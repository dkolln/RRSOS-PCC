namespace RRSOS_PCC.Classes
{
    public static class GroupingHelper
    {
        public static (List<(string Name, int Count)> Left, List<(string Name, int Count)> Right)
            GroupAndSplit<T>(IEnumerable<T> items, Func<T, string> keySelector)
        {
            // 1. Group
            var grouped = items
                .GroupBy(keySelector)
                .Select(g => (Name: g.Key, Count: g.Count()))
                .OrderBy(g => g.Name)
                .ToList();

            // 2. Split into two columns
            int half = (int)Math.Ceiling(grouped.Count / 2.0);

            var left = grouped.Take(half).ToList();
            var right = grouped.Skip(half).ToList();

            return (left, right);
        }

        public static (List<(string Name, int Count)> Left, List<(string Name, int Count)> Right)
            SplitOnly(List<(string Name, int Count)> items)
                {
                    int half = (int)Math.Ceiling(items.Count / 2.0);

                    var left = items.Take(half).ToList();
                    var right = items.Skip(half).ToList();

                    return (left, right);
                }

    }

}
