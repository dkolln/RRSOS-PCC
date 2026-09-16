namespace RRSOS_PCC.Classes
{
    public static class UnknownTypeRegistry
    {
        private static readonly HashSet<string> _unknown = new();

        public static void Register(string gId)
        {
            if (!_unknown.Contains(gId))
                _unknown.Add(gId);
        }

        public static IReadOnlyCollection<string> GetAll() => _unknown;

        public static void Clear() => _unknown.Clear();
    }

}
