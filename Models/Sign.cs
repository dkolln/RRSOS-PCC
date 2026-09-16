namespace RRSOS_PCC.Models
{
    public class Sign
    {
        public long id { get;set;  }
        public string gId { get;set; }
        public string pos { get;set; }
        public string rot { get;set; }
        public long planet { get;set; }
        public string text { get;set; }

        public Position Position { get; private set; } = new Position("0,0,0");
        public Rotation Rotation { get; private set; } = new Rotation("0,0,0,1");

        public void Hydrate()
        {
            if (!string.IsNullOrWhiteSpace(pos))
                Position.Update(pos);

            if (!string.IsNullOrWhiteSpace(rot))
                Rotation.Update(rot);
        }
    }
}
