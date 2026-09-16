using RRSOS_PCC.Classes;
using RRSOS_PCC.Models;
using RRSOS_PCC.ViewModels;
using System.Text;

namespace RRSOS_PCC.Services
{
    public class CardRendererService
    {
        public List<string> RenderCardLines(ViewObject v, int cellWidth)
        {
            string Trunc(string s, int max)
                => string.IsNullOrEmpty(s) ? "" : (s.Length <= max ? s : s[..(max - 1)] + "…");

            string Bar(int count, int size, int width)
            {
                int barWidth = Math.Min(size, width - 10);
                int filled = Math.Min(count, barWidth);
                int empty = barWidth - filled;

                return new string('X', filled) + new string('_', empty);
            }

            var lines = new List<string>();

            // Top border
            lines.Add("┌" + new string('─', cellWidth - 2) + "┐");

            //
            // Header
            //
            var header = v.GId;
            lines.Add(header);// Trunc(header, cellWidth - 2));

            //
            // Metadata
            //
            lines.Add($"Pos: {v.DisplayPosition} => {Math.Round(v.DistanceToPlayer, MidpointRounding.AwayFromZero),1:0.0} m");
            lines.Add($"Slots: {v.UsedSlots} / {v.TotalSlots}");

            lines.Add(TenBar(v.UsedSlots));

            // Divider
            lines.Add(new string('-', cellWidth - 2));

            // Compute all groups
            var allGroups = v.Items
                .GroupBy(i => i.gId)
                .Select(g => new
                {
                    Name = BuildGroupLabel(v.Source as Container, g.Key),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            bool hasMore = allGroups.Count > 3;

            //
            // Top 3 inventory groups
            //
            var groups = allGroups.Take(3).ToList();

            // Header row with optional "+" indicator
            string moreIndicator = hasMore ? " +" : "";
            string headerLine = string.Format("{0,-16} {1,-12} {2,-3}    {3}",
                "Label", "Bar", "Tot", moreIndicator);

            lines.Add(headerLine);


            lines.Add(new string('-', cellWidth - 2));

            foreach (var g in groups)
            {
                if (g.Count < 0)
                {
                    lines.Add("");
                }
                else
                {
                    string name = g.Name;
                    string bar = TenBar(g.Count);

                    // Three columns: label | bar | count
                    // Adjust widths as needed
                    string line = string.Format("{0,-16} {1,-12} {2,-3}",
                        name,
                        bar,
                        g.Count);

                    lines.Add(line);
                }
            }


            //lines.Add("[+] More...");

            // Bottom border


            while (lines.Count < 11)   // adjust to your card height
                lines.Add("--");

            lines.Add("└" + new string('─', cellWidth - 2) + "┘");

            for (int i = 1; i < lines.Count - 1; i++)
            {
                lines[i] = "  " + lines[i];
            }

            return lines;
        }

        private string TenBar(int count)
        {
            int filled = Math.Min(count, 10);
            int empty = 10 - filled;

            return new string('X', filled) + new string('_', empty);
        }

        private string BuildGroupLabel(Container container, string gId)
        {
            var name = gId;
            
            var growable = container?.PrimaryItems?.FirstOrDefault();
            if (growable?.grwth is float g)
                return $"{name} ({g})";

            return name;
        }


    }
}
