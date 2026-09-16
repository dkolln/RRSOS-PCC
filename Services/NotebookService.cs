using RRSOS_PCC.Classes;
using RRSOS_PCC.Models;
using System.Text.Json;

namespace RRSOS_PCC.Services
{
    public class NotebookService
    {
        private readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions { WriteIndented = true };

        public async Task<Notebook> LoadAsync()
        {
            var path = PathResolver.NotebookPath;

            if (!File.Exists(path))
            {
                // Create an empty notebook file if missing
                var empty = new Notebook();
                await SaveAsync(empty);
                return empty;
            }

            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<Notebook>(json) ?? new Notebook();
        }

        public async Task SaveAsync(Notebook notebook)
        {
            var path = PathResolver.NotebookPath;
            var json = JsonSerializer.Serialize(notebook, _jsonOptions);
            await File.WriteAllTextAsync(path, json);
        }
    }
}
