using RRSOS_PCC.Class;
using RRSOS_PCC.Classes;
using RRSOS_PCC.Enums;
using RRSOS_PCC.Models;
using RRSOS_PCC.Services;

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

            // Metadata registry gives you type + category
            var meta = WorldObjectDataService.GetEntry(obj.gId);

            DisplayName = meta.Name == null ? obj.gId : meta.Name + " " + meta.Tier;

            // Enum translation
            // Convert int → enum
            var typeEnum = (WorldObjectType)meta.Type;

            // UI-friendly type string
            Type = typeEnum.ToString();

            // Category via your mapper
            var categoryEnum = WorldObjectResolver.GetCategoryEnum(typeEnum);

            // UI-friendly category string
            Category = categoryEnum.ToString();
        }
    }
}
