namespace RRSOS_PCC.Models
{
    public class Game
    {
        public string SaveFolderPath { get; set; } = "";

        public List<Save> Saves { get; set; } = new();
    }
}
