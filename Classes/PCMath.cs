using RRSOS_PCC.Components.Pages;
using RRSOS_PCC.Enums;
using RRSOS_PCC.Models;
using System.Drawing;
using System.Numerics;
using System.Text.Json;

namespace RRSOS_PCC.Classes
{
    public static class PCMath
    {
        public static Vector3 ParseVector3(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return Vector3.Zero;

            var parts = s.Split(',');

            float x = 0, y = 0, z = 0;

            if (parts.Length > 0 && float.TryParse(parts[0], out float fx)) x = fx;
            if (parts.Length > 1 && float.TryParse(parts[1], out float fy)) y = fy;
            if (parts.Length > 2 && float.TryParse(parts[2], out float fz)) z = fz;

            // If there's a 4th value, ignore it — positions don’t need W
            return new Vector3(x, y, z);
        }

        public static Quaternion ParseQuaternion(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return Quaternion.Identity;

            var parts = s.Split(',');

            float x = 0, y = 0, z = 0, w = 1; // identity fallback

            if (parts.Length > 0 && float.TryParse(parts[0], out float fx)) x = fx;
            if (parts.Length > 1 && float.TryParse(parts[1], out float fy)) y = fy;
            if (parts.Length > 2 && float.TryParse(parts[2], out float fz)) z = fz;
            if (parts.Length > 3 && float.TryParse(parts[3], out float fw)) w = fw;

            return new Quaternion(x, y, z, w);
        }

        public static float GetDistance(Vector2 a, Vector2 b)
        {
            return Vector2.Distance(a, b);
        }

        public static float Deviation(float actualPct, float expectedPct) =>
            actualPct - expectedPct;

        public static float CalculateHeading(Vector2 vector)
        {
            double heading = Math.Atan2(vector.X, vector.Y) * (180.0 / Math.PI);
            if (heading < 0) heading += 360;

            return (float)heading;
        }

        public static Vector2 ForwardFromQuaternion(Quaternion q)
        {
            float x = (float)q.X;
            float y = (float)q.Y;
            float z = (float)q.Z;
            float w = (float)q.W;

            // Unity-style forward vector projected onto XZ plane
            float fx = 2 * (x * z + w * y);
            float fz = 1 - 2 * (x * x + y * y);

            var v = new Vector2(fx, fz);

            if (v.LengthSquared() < 0.0001f)
                return new Vector2(0, 1); // fallback: north

            return Vector2.Normalize(v);
        }

        public static float SafePct(float value, float total) =>
            total <= 0 ? 0 : value / total;

        public static string GetCompassDirection(Vector2 from, Vector2 to)
        {
            var deltaZ = to.Y - from.Y;
            var deltaX = from.X - to.X;

            var angle = MathF.Atan2(deltaX, deltaZ) * (180f / MathF.PI);

            if (angle < 0)
                angle += 360f;

            return angle switch
            {
                >= 337.5f or < 22.5f => "E",
                >= 22.5f and < 67.5f => "NE",
                >= 67.5f and < 112.5f => "N",
                >= 112.5f and < 157.5f => "NW",
                >= 157.5f and < 202.5f => "W",
                >= 202.5f and < 247.5f => "SW",
                >= 247.5f and < 292.5f => "S",
                >= 292.5f and < 337.5f => "SE",
                _ => "?"
            };

        }

        public static string MultiplierAbbreviation(float n)
        {
            double abs = Math.Abs(n);

            if (abs >= 1_000_000_000)
                return (n / 1_000_000_000d).ToString("0.##") + "B";

            if (abs >= 1_000_000)
                return (n / 1_000_000d).ToString("0.##") + "M";

            if (abs >= 1_000)
                return (n / 1_000d).ToString("0.##") + "K";

            return n.ToString("N0");
        }

        public static Base FindNearestBase(
            WorldObject wo,
            SaveState state,
            IReadOnlyDictionary<long, Container> containerById,
            float maxDistance = 100f)
        {
            var bases = state.Bases;

            if (bases == null || bases.Count == 0)
                return null;

            Vector2 effectivePos;

            //
            // 1. If WO is inside a container, use the container's position
            //
            if (wo.Owner.Type == WorldObjectOwnerType.Container && wo.Owner.Id.HasValue)
            {
                if (containerById.TryGetValue(wo.Owner.Id.Value, out var container))
                {
                    effectivePos = container.Position.Flat;
                }
                else
                {
                    // fallback: treat as world-owned
                    return null;
                }
            }
            else
            {
                //
                // 2. Otherwise use the WO's own position
                //
                if (wo.Position?.Flat == null)
                    return null;

                effectivePos = wo.Position.Flat;
            }

            //
            // 3. Find nearest base within 100m
            //
            Base closest = null;
            float maxDistSq = maxDistance * maxDistance;
            float bestDistSq = float.MaxValue;

            foreach (var b in bases)
            {
                float distSq = Vector2.DistanceSquared(effectivePos, b.Position.Flat);

                if (distSq <= maxDistSq && distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    closest = b;
                }
            }

            return closest;
        }
    }
}
