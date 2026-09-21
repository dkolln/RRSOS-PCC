using Microsoft.AspNetCore.Components;
using RRSOS_PCC.Classes;
using RRSOS_PCC.Models;
using RRSOS_PCC.Services;
using RRSOS_PCC.ViewModels;

namespace RRSOS_PCC.Components.Pages
{
    /// <summary>
    /// The one place that listens for new saves and turns them into view models. The child
    /// panels only render what they are handed, so they can never disagree with each other
    /// or show a half-updated state.
    /// </summary>
    public partial class Home : ComponentBase, IDisposable
    {
        // How much closer another base must be before the contents panel switches to it.
        private const float BaseSwitchThreshold = 15f;

        private bool IsLoading = true;
        private Func<Task>? _onChangeHandler;
        private PlayerViewModel? PlayerVM;
        private PlanetViewModel? PlanetVM;
        private VehicleViewModel? VehicleVM;
        private PowerSummaryViewModel? PowerVM;
        private BaseSummaryViewModel? BaseVMs;
        private List<NoteViewModel> NotebookVMs = new();
        private Notebook Notebook = new();
        private List<ExtractorSummaryVM> ExtractorVMs = new();

        private long? _selectedBaseId;

        // Set by clicking a base; null means "show the nearest". Lasts until the page is reloaded.
        private long? _pinnedBaseId;

        /// <summary>The base shown in the contents panel (sticky, see <see cref="BaseSelector"/>).</summary>
        public BaseViewModel? SelectedBase { get; private set; }

        private enum HomeTab { Main, Base, Extractors }

        private HomeTab Tab = HomeTab.Main;

        // Inactive tabs are hidden with CSS, not removed, so they keep their state.
        private string PaneClass(HomeTab tab) => Tab == tab ? "" : "home-hidden";

        //add collapsibles
        private bool ShowPlayer = true;
        private bool ShowPlanet = true;
        private bool ShowVehicle = true;
        private bool ShowBases = true;
        private bool ShowBaseContents = true;
        private bool ShowExtractors = true;
        private bool ShowNotebook = true;

        [Inject] public SaveService SaveSvc { get; set; } = default!;
        [Inject] public NotebookService NotebookSvc { get; set; } = default!;
        [Inject] public PowerService PowerSvc { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            _onChangeHandler = HandleStateChanged;
            SaveSvc.OnChange += _onChangeHandler;

            // No-op if the save is already loaded, in which case no change event will fire,
            // so build from whatever is current instead of waiting for one.
            await SaveSvc.Load();
            await RebuildAsync();

            IsLoading = false;
        }

        // Change events arrive on a background thread; hop onto the renderer's context so the
        // fields are never touched from two threads at once.
        private Task HandleStateChanged() => InvokeAsync(async () =>
        {
            await RebuildAsync();
            StateHasChanged();
        });

        private async Task RebuildAsync()
        {
            var s = SaveSvc.CurrentState;

            PlayerVM = s?.Player != null ? new PlayerViewModel(s.Player) : null;
            PlanetVM = s?.PlanetInfo != null ? new PlanetViewModel(s.PlanetInfo) : null;
            VehicleVM = s?.Vehicle != null ? new VehicleViewModel(s.Vehicle) : null;
            PowerVM = PowerSvc.Summarize(s);

            BaseVMs = s?.Bases != null && s.Player != null
                ? new BaseSummaryViewModel(s.Bases, s.Player)
                : null;

            UpdateSelection();

            ExtractorVMs = s?.Extractors != null
                ? s.Extractors.OrderBy(e => e.Distance).ToList()
                : new List<ExtractorSummaryVM>();

            await ReloadNotebook();
        }

        // A base picked by hand wins; otherwise the contents follow the nearest one (see BaseSelector).
        private void UpdateSelection()
        {
            if (_pinnedBaseId is long pin && BaseVMs?.Bases.Any(b => b.Id == pin) != true)
                _pinnedBaseId = null; // the pinned base is gone (other save, base removed)

            _selectedBaseId = _pinnedBaseId
                ?? BaseSelector.Choose(BaseVMs?.Bases, _selectedBaseId, BaseSwitchThreshold);

            SelectedBase = BaseVMs?.Bases.FirstOrDefault(b => b.Id == _selectedBaseId);
        }

        // Clicking a base pins it; clicking the pinned one again goes back to following the nearest.
        private void TogglePin(long id)
        {
            _pinnedBaseId = _pinnedBaseId == id ? null : id;
            UpdateSelection();
        }

        private void Unpin()
        {
            _pinnedBaseId = null;
            UpdateSelection();
        }

        private async Task ReloadNotebook()
        {
            Notebook = await NotebookSvc.LoadAsync();
            NotebookVMs = Notebook.Notes
                .OrderBy(n => n.Priority)
                .ThenByDescending(n => n.Created)
                .Select(n => new NoteViewModel(n))
                .ToList();
        }

        public void Dispose()
        {
            if (_onChangeHandler != null)
                SaveSvc.OnChange -= _onChangeHandler;
        }
    }
}
