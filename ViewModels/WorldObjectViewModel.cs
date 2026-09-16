using RRSOS_PCC.Models;

namespace RRSOS_PCC.ViewModels
{
    public class WorldObjectViewModel
    {
        public long Id { get; }
        public string GId { get; }

        public string DisplayName { get; }
        public string Type { get; }
        public string Category { get; }

        public WorldObjectViewModel(WorldObject obj)
        {
            Id = obj.id;
            GId = obj.gId;

            // obj.Name/Type/Category/Tier are already resolved via
            // WorldObjectClassifierService during ProcessBinder.BindWorldObjects.
            DisplayName = string.IsNullOrWhiteSpace(obj.Name)
                ? obj.gId
                : string.IsNullOrWhiteSpace(obj.Tier) ? obj.Name : $"{obj.Name} {obj.Tier}";

            Type = obj.Type.ToString();
            Category = obj.Category.ToString();
        }
    }
}
