namespace RRSOS_PCC.Models
{
    public class Note
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public DateTime Created { get; set; }
        public int Priority { get; set; }
    }
}
