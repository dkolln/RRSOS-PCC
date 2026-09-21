using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RRSOS_PCC.Classes
{
    /// <summary>A container label to look for and the item that fills it, e.g. "DaveFood" and "astrofood". Display is the short name for messages.</summary>
    public sealed record ResupplyTarget(string Label, string GId, string? Display = null);

    /// <summary>What happened for one target. Containers is 0 when nothing carries the label.</summary>
    public sealed record ResupplyLine(string Label, string GId, int Containers, int AlreadyFull, int Added);

    public sealed record ResupplyOutcome(string? NewText, IReadOnlyList<ResupplyLine> Lines, IReadOnlyList<string> Problems)
    {
        public bool Changed => NewText != null;
        public bool Failed => Problems.Count > 0;
        public int TotalAdded => Lines.Sum(l => l.Added);
    }

    /// <summary>
    /// Tops up labelled storage containers in the text of a Planet Crafter save.
    ///
    /// A save is sections separated by "@", each holding one JSON object per record. World objects
    /// (which include placed containers and the items lying in them) are records with a gId; what a
    /// container holds is a separate inventory record {"id":liId,"woIds":"id,id,...","size":N}.
    /// To fill a container this adds one item record per free slot, right in front of the container's
    /// own record (so it stays in the same section), and appends the new ids to the inventory.
    ///
    /// It never touches anything else, and proves that before handing the result back: removing the
    /// inserted records and restoring the old id lists must give back the original text exactly.
    /// Any problem means no new text is returned at all. This is pure text-in, text-out so it can be
    /// tested without a game or a file.
    /// </summary>
    public static class SaveResupplier
    {
        // One flat JSON object, aware of strings so braces inside text values cannot end it early.
        // Records in a save never nest objects.
        private static readonly Regex RecordPattern = new(
            @"\{(?:[^{}""]|""(?:[^""\\]|\\.)*"")*\}",
            RegexOptions.Compiled);

        private static readonly Regex WoIdsValue = new(@"(""woIds"":"")[^""]*("")", RegexOptions.Compiled);

        // The range the game hands out ids from for crafted items.
        private const int NewIdMin = 200_000_000;
        private const int NewIdMax = 210_000_000;

        private sealed record Rec(int Start, int Length, string Raw, long Id, string? GId, string? Text, int? LiId, string? WoIds, int? Size)
        {
            public bool IsInventory => WoIds != null;
        }

        private readonly record struct Edit(int Start, int Length, string Replacement);

        /// <summary>What one filled container should look like afterwards, for the self-check.</summary>
        private sealed record Expectation(long InventoryId, string GId, List<long> NewIds, string InsertedText, string OldRaw, string NewRaw);

        public static ResupplyOutcome Apply(string text, IReadOnlyList<ResupplyTarget> targets, Random? random = null)
        {
            random ??= Random.Shared;

            var records = Scan(text, out var problems);
            if (problems.Count > 0)
                return Failed(problems);

            var inventories = new Dictionary<long, Rec>();
            foreach (var inventory in records.Where(r => r.IsInventory))
            {
                if (!inventories.TryAdd(inventory.Id, inventory))
                    return Failed($"Two inventory records share the id {inventory.Id}; the save looks damaged.");
            }

            var used = new HashSet<long>(records.Select(r => r.Id));
            var eol = text.Contains("|\r\n") ? "\r\n" : "\n";

            var edits = new List<Edit>();
            var lines = new List<ResupplyLine>();
            var handled = new HashSet<long>();
            var expectations = new List<Expectation>();

            foreach (var target in targets)
            {
                var containers = records
                    .Where(r => !r.IsInventory && r.LiId is not null
                                && string.Equals(r.Text?.Trim(), target.Label, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var added = 0;
                var full = 0;

                foreach (var container in containers)
                {
                    var liId = container.LiId!.Value;

                    if (!inventories.TryGetValue(liId, out var inventory))
                        return Failed($"Container \"{target.Label}\" points at inventory {liId}, which is not in the save.");

                    if (!handled.Add(liId))
                        continue; // this inventory was already handled under another label

                    var existing = SplitIds(inventory.WoIds!);
                    var free = (inventory.Size ?? 0) - existing.Count;

                    if (free <= 0)
                    {
                        full++;
                        continue;
                    }

                    var newIds = NewIds(free, used, random);

                    // The new records go in front of the container's own record, which always starts
                    // on a record boundary, and each one ends the way every other record does.
                    var items = string.Concat(newIds.Select(id => "{\"id\":" + id + ",\"gId\":\"" + target.GId + "\"}|" + eol));
                    var updated = WoIdsValue.Replace(inventory.Raw, m => m.Groups[1].Value + string.Join(",", existing.Concat(newIds)) + m.Groups[2].Value, 1);

                    edits.Add(new Edit(container.Start, 0, items));
                    edits.Add(new Edit(inventory.Start, inventory.Length, updated));
                    expectations.Add(new Expectation(liId, target.GId, newIds, items, inventory.Raw, updated));
                    added += newIds.Count;
                }

                lines.Add(new ResupplyLine(target.Label, target.GId, containers.Count, full, added));
            }

            if (edits.Count == 0)
                return new ResupplyOutcome(null, lines, Array.Empty<string>());

            // Apply from the end of the text backwards so earlier positions stay valid.
            var result = new StringBuilder(text);
            foreach (var edit in edits.OrderByDescending(e => e.Start))
            {
                result.Remove(edit.Start, edit.Length);
                result.Insert(edit.Start, edit.Replacement);
            }

            var newText = result.ToString();
            var check = Verify(text, newText, expectations);

            return check.Count > 0 ? new ResupplyOutcome(null, lines, check) : new ResupplyOutcome(newText, lines, Array.Empty<string>());
        }

        // ------------------------------------------------------------------

        private static ResupplyOutcome Failed(string problem) => new(null, Array.Empty<ResupplyLine>(), new[] { problem });

        private static ResupplyOutcome Failed(List<string> problems) => new(null, Array.Empty<ResupplyLine>(), problems);

        private static List<long> SplitIds(string woIds) =>
            woIds.Length == 0 ? new() : woIds.Split(',').Select(long.Parse).ToList();

        private static List<long> NewIds(int count, HashSet<long> used, Random random)
        {
            var ids = new List<long>(count);
            while (ids.Count < count)
            {
                var candidate = random.NextInt64(NewIdMin, NewIdMax);
                if (used.Add(candidate))
                    ids.Add(candidate);
            }

            return ids;
        }

        /// <summary>Finds and parses every record. Anything that is not valid JSON is a problem.</summary>
        private static List<Rec> Scan(string text, out List<string> problems)
        {
            problems = new List<string>();
            var records = new List<Rec>();

            foreach (Match match in RecordPattern.Matches(text))
            {
                try
                {
                    using var doc = JsonDocument.Parse(match.Value);
                    var root = doc.RootElement;

                    if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("id", out var idElement) || !idElement.TryGetInt64(out var id))
                        continue; // a record without an id (settings, messages, ...) is none of our business

                    string? Str(string name) => root.TryGetProperty(name, out var e) && e.ValueKind == JsonValueKind.String ? e.GetString() : null;
                    int? Int(string name) => root.TryGetProperty(name, out var e) && e.ValueKind == JsonValueKind.Number && e.TryGetInt32(out var v) ? v : null;

                    records.Add(new Rec(match.Index, match.Length, match.Value, id, Str("gId"), Str("text"), Int("liId"), Str("woIds"), Int("size")));
                }
                catch (JsonException ex)
                {
                    problems.Add($"A record at position {match.Index} is not valid JSON: {ex.Message}");
                }
            }

            return records;
        }

        /// <summary>Checks the edited text against the original before anyone is allowed to write it.</summary>
        private static List<string> Verify(string before, string after, List<Expectation> expectations)
        {
            var problems = new List<string>();

            var records = Scan(after, out var scanProblems);
            problems.AddRange(scanProblems);
            if (problems.Count > 0)
                return problems;

            // 1. The strong check: take the inserted records back out and put the old id lists back,
            //    and the original must come back byte for byte.
            var restored = after;
            foreach (var e in expectations)
            {
                var at = restored.IndexOf(e.InsertedText, StringComparison.Ordinal);
                if (at < 0)
                {
                    problems.Add($"Inserted items for inventory {e.InventoryId} were not found in the result.");
                    continue;
                }

                restored = restored.Remove(at, e.InsertedText.Length);

                var raw = restored.IndexOf(e.NewRaw, StringComparison.Ordinal);
                if (raw < 0)
                {
                    problems.Add($"Updated inventory {e.InventoryId} was not found in the result.");
                    continue;
                }

                restored = restored.Remove(raw, e.NewRaw.Length).Insert(raw, e.OldRaw);
            }

            if (problems.Count == 0 && !string.Equals(restored, before, StringComparison.Ordinal))
                problems.Add("Undoing the edit does not give back the original save, so something else changed. Nothing was written.");

            // 2. The structural checks: what the game will actually read.
            var objects = new Dictionary<long, Rec>();
            foreach (var obj in records.Where(r => !r.IsInventory && r.GId != null))
            {
                if (!objects.TryAdd(obj.Id, obj))
                    problems.Add($"Object id {obj.Id} appears twice after the edit.");
            }

            // Objects and inventories can share an id number by coincidence, so each kind is checked on its own.
            var inventories = records.Where(r => r.IsInventory).GroupBy(r => r.Id).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var e in expectations)
            {
                if (!inventories.TryGetValue(e.InventoryId, out var found) || found.Count != 1)
                {
                    problems.Add($"Inventory {e.InventoryId} is missing or duplicated after the edit.");
                    continue;
                }

                var held = SplitIds(found[0].WoIds!);

                if (held.Count > (found[0].Size ?? 0))
                    problems.Add($"Inventory {e.InventoryId} would hold {held.Count} items in {found[0].Size} slots.");

                foreach (var id in e.NewIds)
                {
                    if (!held.Contains(id))
                        problems.Add($"New item {id} is not listed in inventory {e.InventoryId}.");

                    if (!objects.TryGetValue(id, out var item) || item.GId != e.GId)
                        problems.Add($"New item {id} is missing or is not a {e.GId}.");
                }
            }

            return problems;
        }
    }
}
