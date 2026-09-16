using RRSOS_PCC.Enums;
using RRSOS_PCC.Models;

namespace RRSOS_PCC.Classes
{
    public static class PanelParser
    {
        public static List<Panel> Parse(string pnls)
        {
            var panels = new List<Panel>();

            if (string.IsNullOrWhiteSpace(pnls))
                return panels;

            var rawValues = pnls
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToArray();

            for (int i = 0; i < rawValues.Length; i++)
            {
                int raw = rawValues[i];

                panels.Add(new Panel
                {
                    Index = i,
                    RawValue = raw,
                    Direction = (PanelDirection)i,
                    Type = ResolveType(i),
                    Module = ResolveModule(raw)
                });
            }

            return panels;
        }

        private static PanelType ResolveType(int index)
        {
            return index switch
            {
                0 => PanelType.Wall,
                1 => PanelType.Wall,
                2 => PanelType.Wall,
                3 => PanelType.Wall,
                4 => PanelType.Ceiling,
                5 => PanelType.Floor,
                _ => PanelType.Unknown
            };
        }

        private static PanelModule ResolveModule(int raw)
        {
            return raw switch
            {
                4 => PanelModule.Entrance,
                2 => PanelModule.Connection,
                3 => PanelModule.Window,
                _ => PanelModule.None
            };
        }
    }
}
