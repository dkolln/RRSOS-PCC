namespace RRSOS_PCC.Models
{
    public class PlanetColorLayer
    {
        public string layerId { get; set; } = "";
        public int planet { get; set; }
        public string colorBase { get; set; } = "";
        public string colorCustom { get; set; } = "";
        public float colorBaseLerp { get; set; }
        public float colorCustomLerp { get; set; }

        // Derived fields (optional)
        public float[] ColorBaseRGBA =>
            colorBase.Split('-').Select(float.Parse).ToArray();

        public float[] ColorCustomRGBA =>
            colorCustom.Split('-').Select(float.Parse).ToArray();
    }
}
