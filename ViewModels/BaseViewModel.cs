using RRSOS_PCC.Classes;
using RRSOS_PCC.Enums;
using RRSOS_PCC.Models;
using System.Numerics;

namespace RRSOS_PCC.ViewModels
{
    public class BaseViewModel
    {
        private readonly Base _model;
        private readonly Position _playerPos;

        // These lean directly on the model, no extra memory used for local fields
        public long Id => _model.id;
        public string Name => _model.Name;
        public BaseType Type => _model.Type;

        public string DisplayPosition => $"({_model.Position.Flat.X:0},{_model.Position.Flat.Y:0})"; 
        public float Distance => Vector2.Distance(_model.Position.Flat, _playerPos.Flat);
        public string Direction => PCMath.GetCompassDirection(_playerPos.Flat, _model.Position.Flat);
        public string DisplayDistance => $"{(int)Distance:0.#}m";

        // The Constructor "Binds" the data to the View
        public BaseViewModel(Base b, Position playerPos)
        {
            _model = b ?? throw new ArgumentNullException(nameof(b));
            _playerPos = playerPos ?? throw new ArgumentNullException(nameof(playerPos));
        }

        public IEnumerable<InventoryGroup> Groups => _model.Inventory
            .Select(kvp => new InventoryGroup(kvp.Key, kvp.Value));
    }

}
