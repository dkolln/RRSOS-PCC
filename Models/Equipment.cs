namespace RRSOS_PCC.Models
{
    public class Equipment
    {
        public int Id { get; set; }
        public int Capacity { get; set; }
        public List<int> ObjectIds { get; set; } = new();
        public List<WorldObject> Items { get; set; } = new();

        public ContainerDetail ContainerDetail { get; set; }
    }
}
