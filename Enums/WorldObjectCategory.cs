using System.ComponentModel;

namespace RRSOS_PCC.Enums
{
    public enum WorldObjectCategory
    {
        None = 0,

        [Description("Equipment")]
        Equipment = 1000,

        [Description("Resource")]
        Resource = 2000,

        [Description("Biological")]
        Biological = 3000,

        [Description("Consumable")]
        Consumable = 4000,

        [Description("Machine")]
        Machine = 5000,

        [Description("Base Part")]
        BasePart = 6000,

        [Description("Component")]
        Component = 7000,

        [Description("Modifier")]
        Modifier = 8000,

        [Description("Container")]
        Container = 9000,

        [Description("World Marker")]
        WorldMarker = 10000,

        [Description("Utility Item")]
        UtilityItem = 11000,

        [Description("Wreck")]
        Wreck = 12000,

        [Description("Unknown")]
        Unknown = 9999
    }
}
