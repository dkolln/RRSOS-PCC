using Microsoft.AspNetCore.Components;
using RRSOS_PCC.Enums;
using RRSOS_PCC.Services;
using RRSOS_PCC.ViewModels;
using System.Diagnostics;

namespace RRSOS_PCC.Components.Pages
{
    public partial class Extractors : ComponentBase, IDisposable
    {
        [Inject] public SaveService SaveSvc { get; set; }
        public ExtractorGroupVM? ActiveGroup { get; set; }
        private Dictionary<ExtractorType, bool> groupExpanded = new();
        private Dictionary<string, bool> subExpanded = new();

        public List<ExtractorGroupVM> ExtractorGroups { get; set; } = new();
        public List<ExtractorSummaryVM> ExtractorList { get; set; } = new();
        private Dictionary<long, bool> expanded = new();

        private Func<Task>? _onChangeHandler;

        protected override void OnInitialized()
        {
            LoadExtractors();

            _onChangeHandler = RefreshContentsAsync;
            SaveSvc.OnChange += _onChangeHandler;
        }

        private async Task RefreshContentsAsync()
        {
            LoadExtractors();
            await InvokeAsync(StateHasChanged);
        }

        private void LoadExtractors()
        {
            ExtractorList = SaveSvc.CurrentState?.Extractors != null
                ? new List<ExtractorSummaryVM>(SaveSvc.CurrentState.Extractors)
                : new List<ExtractorSummaryVM>();

            ExtractorList = ExtractorList
                .OrderBy(x => x.Distance)
                .ToList();

            // NEW: build grouped extractors
            ExtractorGroups = ExtractorList
                .GroupBy(e => e.Type)
                .Select(g => new ExtractorGroupVM
                {
                    Type = g.Key,
                    DisplayName = g.Key.ToString(),
                    TotalItems = g.Sum(x => x.Count),
                    FullExtractors = g.Count(x => x.IsFull),
                    TotalExtractors = g.Count(),
                    Extractors = g.ToList()
                })
                .ToList();

            foreach (var group in ExtractorGroups)
            {
                if (group.Type == ExtractorType.Ore)
                {
                    group.SubGroups = group.Extractors
                        .GroupBy(e => e.ProductGroup)
                        .Select(sg => new ExtractorSubGroupVM
                        {
                            ProductGroup = sg.Key,
                            Extractors = sg.ToList(),
                            TotalItems = sg.Sum(x => x.Count),
                            FullExtractors = sg.Count(x => x.IsFull),
                            TotalExtractors = sg.Count()
                        })
                        .OrderBy(sg => sg.ProductGroup)
                        .ToList();
                }
            }
        }

        private void OpenGroup(ExtractorGroupVM group)
        {
            ActiveGroup = group;
        }

        private void CloseGroup()
        {
            ActiveGroup = null;
        }

        private void ToggleGroup(ExtractorGroupVM group)
        {
            if (!groupExpanded.ContainsKey(group.Type))
                groupExpanded[group.Type] = true;
            else
                groupExpanded[group.Type] = !groupExpanded[group.Type];
        }

        private void ToggleSub(ExtractorSubGroupVM sub)
        {
            if (!subExpanded.ContainsKey(sub.ProductGroup))
                subExpanded[sub.ProductGroup] = true;
            else
                subExpanded[sub.ProductGroup] = !subExpanded[sub.ProductGroup];
        }


        public void Dispose()
        {
            if (_onChangeHandler != null)
                SaveSvc.OnChange -= _onChangeHandler;
        }
    }
}
