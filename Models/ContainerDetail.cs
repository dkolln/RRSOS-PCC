using RRSOS_PCC.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RRSOS_PCC.Models
{
    public class ContainerDetail
    {
        private List<int>? _itemIds;

        public long id { get; set; }
        public string woIds { get; set; }
        public int? size { get; set; }

        public List<int> ItemIds => _itemIds ??= GetItemIds();

        public List<int> GetItemIds()
        {
            if (_itemIds != null)
                return _itemIds;

            if (string.IsNullOrWhiteSpace(woIds))
                return _itemIds = new();

            return _itemIds = woIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var id) ? id : -1)
                .Where(id => id >= 0)
                .ToList();
        }
    }
}
