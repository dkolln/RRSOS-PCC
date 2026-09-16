using RRSOS_PCC.Models;

namespace RRSOS_PCC.ViewModels
{
    public class EquipmentViewModel
    {
        private readonly Equipment _equipment;

        public EquipmentViewModel(Equipment equipment)
        {
            _equipment = equipment;
        }

        public int Count => _equipment.Items.Count;

        public IEnumerable<WorldObject> Items =>
            _equipment.Items;

        // Split into two columns for UI
        public IEnumerable<WorldObject> LeftColumn =>
            _equipment.Items.Take((_equipment.Items.Count + 1) / 2);

        public IEnumerable<WorldObject> RightColumn =>
            _equipment.Items.Skip((_equipment.Items.Count + 1) / 2);

        // Optional: capacity display
        public string CapacityDisplay =>
            $"{_equipment.Items.Count}/{_equipment.Capacity}";
    }
}
