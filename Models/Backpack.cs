namespace RRSOS_PCC.Models
{
    public class Backpack
    {
        public int Id { get; set; }
        public int Capacity { get; set; }
        public List<WorldObject> Items { get; set; } = new();
    }
}
