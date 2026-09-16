using RRSOS_PCC.Models;

public class NoteViewModel
{
    public Note Model { get; }

    public NoteViewModel(Note model)
    {
        Model = model;
    }

    public int Id => Model.Id;
    public string Text => Model.Text;
    public int Priority => Model.Priority;
    public DateTime Created => Model.Created;

    // UI formatting
    public string CreatedDisplay =>
        Model.Created.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

    // UI-only flags
    public bool IsEditing { get; set; }
}
