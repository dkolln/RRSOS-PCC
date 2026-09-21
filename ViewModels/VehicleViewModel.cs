using RRSOS_PCC.Models;
using System.Numerics;

namespace RRSOS_PCC.ViewModels
{
    public class VehicleViewModel
    {
        public int Id { get; }
        public string GId { get; }
        public string Name { get; }
        public string Type { get; }

        public PositionViewModel Position { get; }
        public RotationViewModel Rotation { get; }

        // A stowed truck has no pos/rot in the save at all; Position then holds a made-up 0,0,0.
        public bool HasPosition { get; }

        public IReadOnlyList<WorldObjectViewModel> Trunk { get; }
        public IReadOnlyList<WorldObjectViewModel> Modules { get; }

        public VehicleViewModel(Vehicle v)
        {
            Id = v.id;
            GId = v.gId;

            // You can refine these once you have metadata
            Name = v.gId;
            Type = v.gId;

            HasPosition = !string.IsNullOrWhiteSpace(v.pos);
            Position = new PositionViewModel(v.Position);
            Rotation = new RotationViewModel(v.Rotation);

            Trunk = v.TrunkItems
                .Select(o => new WorldObjectViewModel(o))
                .ToList();

            Modules = v.ModuleItems
                .Select(o => new WorldObjectViewModel(o))
                .ToList();
        }
    }
}
