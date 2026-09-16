namespace RRSOS_PCC.ViewModels
{
    public class CategoryGroupViewModel
    {
        public string CategoryName { get; set; } = "";
        public List<(string Name, int Count)> Items { get; set; } = new();

        public List<(string Name, int Count)> Left { get; set; } = new();
        public List<(string Name, int Count)> Right { get; set; } = new();
    }

}
