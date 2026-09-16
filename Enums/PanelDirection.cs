using System.ComponentModel;

namespace RRSOS_PCC.Enums
{
    public enum PanelDirection
    {
        [Description("East")] PosX = 0,   // East
        [Description("West")] NegX = 1,   // West
        [Description("North")] PosZ = 2,   // North
        [Description("South")] NegZ = 3,   // South
        [Description("Top")] PosY = 4,   // Top
        [Description("Bottom")] NegY = 5,   // Bottom

        Unknown = 99
    }
}
