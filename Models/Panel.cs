using RRSOS_PCC.Enums;

namespace RRSOS_PCC.Models
{
    public class Panel
    {
        public int Index { get; init; }
        public PanelType Type { get; init; }
        
        public PanelDirection Direction { get; init; }
        
        public PanelModule Module { get; set; } = PanelModule.Unknown;
        
        public int RawValue { get; init; }

        public bool IsConnected => Module == PanelModule.Connection;
        public bool HasDoor => Module == PanelModule.Entrance;

    }

}
