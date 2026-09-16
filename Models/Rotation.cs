using RRSOS_PCC.Classes;
using System.Numerics;

namespace RRSOS_PCC.Models
{
    public class Rotation
    {
        public Quaternion RotationQuat { get; private set; } = Quaternion.Identity;

        public Rotation(string raw)
        {
            Update(raw);
        }

        public void Update(string raw)
        {
            RotationQuat = PCMath.ParseQuaternion(raw);
        }

        // 2D forward vector (XZ plane)
        public Vector2 Forward2D
        {
            get
            {
                var fwd = PCMath.ForwardFromQuaternion(RotationQuat);
                return new Vector2(fwd.X, fwd.Y);
            }
        }

        // Heading in degrees (0–360)
        public float HeadingDegrees =>
            PCMath.CalculateHeading(Forward2D);
    }
}
