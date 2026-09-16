using System.Numerics;

namespace RRSOS_PCC.ViewModels
{
    public class PositionViewModel
    {
        private readonly Position _pos;

        public PositionViewModel(Position pos)
        {
            _pos = pos;
        }

        public Vector3 Full => _pos.Full;
        public Vector2 Flat => _pos.Flat;

        public string Display2D => $"({_pos.Flat.X:F1}, {_pos.Flat.Y:F1})";
        public string Display3D => $"({_pos.Full.X:F2}, {_pos.Full.Y:F2}, {_pos.Full.Z:F2})";
        public string DisplayElevation => $"{_pos.Elevation:F1}";
        public string DisplayPosEl => $"{Display2D}  El={DisplayElevation}";
    }

}
