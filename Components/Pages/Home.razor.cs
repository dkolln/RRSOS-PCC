using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using RRSOS_PCC.Models;
using RRSOS_PCC.Services;
using RRSOS_PCC.ViewModels;
using System.Net.ServerSentEvents;
using System.Numerics;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;

namespace RRSOS_PCC.Components.Pages
{
    public partial class Home : ComponentBase, IDisposable
    {
        private bool IsLoading = true;
        private Func<Task>? _onChangeHandler;
        private PlayerViewModel? PlayerVM;
        private PlanetViewModel? PlanetVM;
        private VehicleViewModel? VehicleVM;
        private BaseSummaryViewModel? BaseVMs;
        private List<NoteViewModel> NotebookVMs = new();
        private Notebook Notebook = new();

        public BaseViewModel? ClosestBase => BaseVMs?.Bases.OrderBy(b => b.Distance).FirstOrDefault();
        private List<ExtractorSummaryVM>? ExtractorVMs;
        // private List<ContainerViewModel>? ContainerVMs;


        //add collapsibles
        private bool ShowPlayer = true;
        private bool ShowPlanet = true;
        private bool ShowVehicle = true;
        private bool ShowBases = true;
        private bool ShowBaseContents = true;
        private bool ShowExtractors = true;
        private bool ShowNotebook = true;

        [Inject] public SaveService SaveSvc { get; set; }
        [Inject] public NotebookService NotebookSvc { get; set; }
        [Inject] public GameMathService GameMathSvc { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _onChangeHandler = async () => await HandleStateChanged();
            SaveSvc.OnChange += _onChangeHandler;

            await SaveSvc.Load();

            Notebook = await NotebookSvc.LoadAsync();
            NotebookVMs = Notebook.Notes
                .OrderBy(n => n.Priority)
                .Select(n => new NoteViewModel(n))
                .ToList();

            IsLoading = false;
        }


        protected override void OnAfterRender(bool firstRender)
        {
        }

        private async Task HandleStateChanged()
        {
            var s = SaveSvc.CurrentState;

            PlayerVM = s?.Player != null ? new PlayerViewModel(s.Player) : null;
            PlanetVM = s?.PlanetInfo != null ? new PlanetViewModel(s.PlanetInfo) : null;
            VehicleVM = s?.Vehicle != null ? new VehicleViewModel(s.Vehicle) : null;

            BaseVMs = s?.Bases != null && s?.Player != null
                ? new BaseSummaryViewModel(s.Bases, s.Player)
                : null;

            ExtractorVMs = s?.Extractors != null
                ? new List<ExtractorSummaryVM>(s.Extractors)
                : new List<ExtractorSummaryVM>();

            Notebook = await NotebookSvc.LoadAsync();
            NotebookVMs = Notebook.Notes
                .OrderBy(n => n.Priority)
                .ThenByDescending(n => n.Created)
                .Select(n => new NoteViewModel(n))
                .ToList();


            InvokeAsync(StateHasChanged);

        }

        private async Task ReloadNotebook()
        {
            Notebook = await NotebookSvc.LoadAsync();
            NotebookVMs = Notebook.Notes
                .OrderBy(n => n.Priority)
                .ThenBy(n => n.Created)
                .Select(n => new NoteViewModel(n))
                .ToList();

            StateHasChanged();
        }

        public void Dispose()
        {
            if (_onChangeHandler != null) SaveSvc.OnChange -= _onChangeHandler;
        }



        //protected override void OnParametersSet()
        //{
        //    HandleStateChanged();
        //}

    }
}
