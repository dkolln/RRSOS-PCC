using RRSOS_PCC.Classes;

namespace RRSOS_PCC.Models
{
    public class Pod : WorldObject
    {
        public string pnls { get; set; } = "";
        public List<Panel> Panels { get; private set; } = new();

        public void BuildPanels()
        {
            Panels = PanelParser.Parse(pnls);
        }
    }


}
