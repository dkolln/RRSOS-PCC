using Microsoft.AspNetCore.Components;
using RRSOS_PCC.Models;
using RRSOS_PCC.Services;
using RRSOS_PCC.ViewModels;

namespace RRSOS_PCC.Components.Pages
{
    public partial class PlanetSummary : ComponentBase
    {
        [Parameter] 
        public PlanetViewModel? Data { get; set; }

        [Inject]
        public SaveService SaveSvc { get; set; }

        [Inject]
        public GameMathService GameMathSvc { get; set; }

        protected override void OnParametersSet()
        {
        }
    }
}
