using RRSOS_PCC.Models;

namespace RRSOS_PCC.ViewModels
{
    public class RotationViewModel
    {
        private readonly Rotation _rot;

        public RotationViewModel(Rotation rot)
        {
            _rot = rot;
        }

        // UI-friendly heading (e.g., "123.4°")
        public string Heading => $"{_rot.HeadingDegrees:F1}°";

        // Optional: quaternion display for debugging or advanced UI
        public string Quaternion =>
            $"({_rot.RotationQuat.X:F3}, {_rot.RotationQuat.Y:F3}, {_rot.RotationQuat.Z:F3}, {_rot.RotationQuat.W:F3})";
    }
}