using RRSOS_PCC.Enums;

namespace RRSOS_PCC.Classes.Interfaces
{
    public interface IObjectClass
    {
        string Name { get; }
        int Value { get; }
        GroupMode Group { get; }
    }

}
