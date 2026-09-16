using Microsoft.AspNetCore.Components;
using RRSOS_PCC.Models;
using RRSOS_PCC.ViewModels;
using RRSOS_PCC.Services;
using RRSOS_PCC.Reccords;

namespace RRSOS_PCC.Components.Pages
{
    public partial class NoteSummary : ComponentBase
    {
        [Parameter] public List<NoteViewModel> Notes { get; set; }
        [Parameter] public EventCallback OnNotesChanged { get; set; }
        [Parameter] public Notebook NotebookData { get; set; }


        private string newNoteText = string.Empty;
        private bool isAddingNote = false;

        private bool ConfirmAddWardenKeys = false;


        private async Task SaveNotebook()
        {
            await NotebookService.SaveAsync(NotebookData);
            await OnNotesChanged.InvokeAsync();
            StateHasChanged();
        }


        // -----------------------------
        // UI Actions
        // -----------------------------
        private void ShowAddNote()
        {
            isAddingNote = true;
        }

        private async Task AddNote()
        {
            if (string.IsNullOrWhiteSpace(newNoteText))
            {
                isAddingNote = false;
                return;
            }

            var nextId = NotebookData.Notes.Count == 0
                ? 1
                : NotebookData.Notes.Max(n => n.Id) + 1;

            var nextPriority = NotebookData.Notes.Count == 0
                ? 1
                : NotebookData.Notes.Max(n => n.Priority) + 1;

            NotebookData.Notes.Add(new Note
            {
                Id = nextId,
                Text = newNoteText.Trim(),
                Created = DateTime.UtcNow,
                Priority = nextPriority
            });

            newNoteText = string.Empty;
            isAddingNote = false;

            await SaveNotebook();
        }

        private async Task MoveUp(NoteViewModel vm)
        {
            var notes = NotebookData.Notes.OrderBy(n => n.Priority).ToList();
            var index = notes.IndexOf(vm.Model);

            if (index > 0)
            {
                var above = notes[index - 1];

                // swap priorities
                int temp = vm.Model.Priority;
                vm.Model.Priority = above.Priority;
                above.Priority = temp;

                await SaveNotebook();
            }
        }


        private async Task MoveDown(NoteViewModel vm)
        {
            var notes = NotebookData.Notes.OrderBy(n => n.Priority).ToList();
            var index = notes.IndexOf(vm.Model);

            if (index < notes.Count - 1)
            {
                var below = notes[index + 1];

                // swap priorities
                int temp = vm.Model.Priority;
                vm.Model.Priority = below.Priority;
                below.Priority = temp;

                await SaveNotebook();
            }
        }


        private async Task DeleteNote(NoteViewModel vm)
        {
            NotebookData.Notes.Remove(vm.Model);
            await SaveNotebook();
        }

        private async Task AddWardenKeys()
        {
            foreach (var key in WardenKeys.Keys)
            {
                var nextId = NotebookData.Notes.Count == 0
                    ? 1
                    : NotebookData.Notes.Max(n => n.Id) + 1;

                var nextPriority = NotebookData.Notes.Count == 0
                    ? 1
                    : NotebookData.Notes.Max(n => n.Priority) + 1;

                // Format: "x,y,z"
                var pos = $"{key.Location.Full.X},{key.Location.Full.Y},{key.Location.Full.Z}";

                NotebookData.Notes.Add(new Note
                {
                    Id = nextId,
                    Text = $"Warden Key {key.Id}: {pos}",
                    Created = DateTime.UtcNow,
                    Priority = nextPriority
                });
            }

            await SaveNotebook();
        }

        private void TryAddWardenKeys()
        {
            bool anyExist = NotebookData.Notes
                .Any(n => n.Text.StartsWith("Warden Key"));

            if (anyExist)
            {
                ConfirmAddWardenKeys = true;
                StateHasChanged();   // 🔥 Forces UI to show the confirmation box
            }
            else
            {
                _ = AddWardenKeys();
            }
        }


    }
}
