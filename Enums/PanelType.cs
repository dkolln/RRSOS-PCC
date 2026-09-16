using System.ComponentModel;

namespace RRSOS_PCC.Enums
{
    public enum PanelType
    {
        [Description("None")]
        None = 0,

        [Description("Wall")]
        Wall = 1,

        [Description("Ceiling")]
        Ceiling= 4,

        [Description("Floor")]
        Floor = 5,

        [Description("Unknown")]
        Unknown = 99
    }
}
