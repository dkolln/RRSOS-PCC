using RRSOS_PCC.Models;
using System.Numerics;

namespace RRSOS_PCC.ViewModels
{
    public class ViewObject
    {
        // Identity
        public long Id { get; init; }
        public string GId { get; init; }
        public string DisplayName { get; init; }

        // Position
        public Vector3 Position { get; init; }
        public string DisplayPosition { get; init; }
        public float DistanceToPlayer { get; init; }

        // Inventory summary
        public int UsedSlots { get; init; }
        public int TotalSlots { get; init; }

        // Items (already hydrated)
        public IReadOnlyList<WorldObject> Items { get; init; }

        // Source domain object (Container, Vehicle, Extractor, etc.)
        public object Source { get; init; }
    }

}
