using Microsoft.AspNetCore.Components;
using RRSOS_PCC.ViewModels;
using RRSOS_PCC.Classes;
using RRSOS_PCC.Enums;
using RRSOS_PCC.Models;
using System.Numerics;

namespace RRSOS_PCC.Components.Pages
{
    public partial class BaseSummary : ComponentBase, IDisposable
    {
        private int showNumberBases = 100;

        [Parameter]
        public BaseSummaryViewModel? Data { get; set; }

        public BaseViewModel? SelectedBase { get; set; }

        private List<BaseViewModel> DisplayList = new();

        private const float SwitchThreshold = 15f;

        private Func<Task>? _onChangeHandler;

        protected override void OnInitialized()
        {
            _onChangeHandler = RefreshSummaryAsync;
            SaveSvc.OnChange += _onChangeHandler;
        }

        protected override void OnParametersSet()
        {
            if (Data?.Bases == null || !Data.Bases.Any())
            {
                DisplayList.Clear();
                SelectedBase = null;
                return;
            }

            DisplayList = Data.Bases.Take(showNumberBases).ToList();

            UpdateSelectionLogic(DisplayList[0]);
        }

        private async Task RefreshSummaryAsync()
        {
            if (Data?.Bases == null || !Data.Bases.Any())
                return;

            DisplayList = Data.Bases.Take(showNumberBases).ToList();

            UpdateSelectionLogic(DisplayList[0]);

            await InvokeAsync(StateHasChanged);
        }

        private void UpdateSelectionLogic(BaseViewModel closest)
        {
            if (SelectedBase == null)
            {
                SelectedBase = closest;
                return;
            }

            if (SelectedBase.Id == closest.Id)
            {
                SelectedBase = closest;
                return;
            }

            if (closest.Distance < (SelectedBase.Distance - SwitchThreshold))
            {
                SelectedBase = closest;
            }
        }

        public void Dispose()
        {
            if (_onChangeHandler != null)
                SaveSvc.OnChange -= _onChangeHandler;
        }
    }
}
