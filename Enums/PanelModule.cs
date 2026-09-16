using System.ComponentModel;

namespace RRSOS_PCC.Enums
{
    public enum PanelModule
    {
        [Description("None")]
        None = 0,
        [Description("Door/Entranceway")]
        Entrance = 1,
        [Description("Connection")]
        Connection = 2,
        [Description("Window")]
        Window = 3,
        [Description("Unknown")]
        Unknown = 99,
    }
}
