using RRSOS_PCC.Classes;
using System.Numerics;

public class Position
{
    public Vector3 Full { get; private set; }

    // 2D projection (X,Z)
    public Vector2 Flat => new(Full.X, Full.Z);

    // Elevation (Y)
    public float Elevation => Full.Y;

    public Position(string raw)
    {
        Update(raw);
    }

    public void Update(string raw)
    {
        Full = PCMath.ParseVector3(raw);
    }

    // --- Distance Helpers ---

    public float DistanceTo(Position other)
        => Vector3.Distance(Full, other.Full);

    public float Distance2D(Position other)
        => Vector2.Distance(Flat, other.Flat);
}
