namespace RRSOS_PCC.Reccords
{
    public record WardenKey(int Id, Position Location);

    public static class WardenKeys
    {
        public static readonly List<WardenKey> Keys = new()
    {
        new(1,  new Position("1895,75,266")),
        new(2,  new Position("-1433,118,555")),
        new(3,  new Position("-1328,91,1325")),
        new(4,  new Position("-382,28,1495")),
        new(5,  new Position("2375,97,1009")),
        new(6,  new Position("1435,75,-758")),
        new(7,  new Position("-925,88,-627")),
        new(8,  new Position("-303,84,614")),
        new(9,  new Position("1586,51,560")),
        new(10, new Position("714,67,1709")),
        new(11, new Position("1316,7,1503")),
        new(12, new Position("1536,22,2219")),
        new(13, new Position("1257,17,249"))
    };
    }

}
