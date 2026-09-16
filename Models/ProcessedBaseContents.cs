using RRSOS_PCC.ViewModels;

namespace RRSOS_PCC.Models
{
    public class ProcessedBaseContents
    {
        // Items inside any container, grouped by their WorldObjectCategory
        public List<CategoryGroupViewModel> CategorizedItems { get; set; } = new();

        // Loose items on the ground within the radius
        public List<(string Name, int Count)> Boneyard { get; set; } = new();
        public List<(string Name, int Count)> BoneyardLeft { get; set; } = new();
        public List<(string Name, int Count)> BoneyardRight { get; set; } = new();

    }
}
