using RRSOS_PCC.Models;

namespace RRSOS_PCC.ViewModels
{
    public class BackpackViewModel
    {
        private readonly Backpack _backpack;

        public BackpackViewModel(Backpack backpack)
        {
            _backpack = backpack;
        }

        public int Count => _backpack.Items.Count;

        public IEnumerable<WorldObject> Items =>
            _backpack.Items;

        // Split into two columns for UI
        public IEnumerable<WorldObject> LeftColumn =>
            _backpack.Items.Take((_backpack.Items.Count + 1) / 2);

        public IEnumerable<WorldObject> RightColumn =>
            _backpack.Items.Skip((_backpack.Items.Count + 1) / 2);

        // Optional: capacity display
        public string CapacityDisplay =>
            $"{_backpack.Items.Count}/{_backpack.Capacity}";
    }
}
