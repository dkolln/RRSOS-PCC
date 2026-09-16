using System.Text.Json;

namespace RRSOS_PCC.Models
{
    public class GlobalProgression
    {
        public int terraTokens { get; set; }
        public int allTimeTerraTokens { get; set; }
        public string? unlockedGroups { get; set; }
        public int openedInstanceSeed { get; set; }
        public int openedInstanceTimeLeft { get; set; }
    }
}
