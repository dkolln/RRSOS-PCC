namespace RRSOS_PCC.Models
{
    public class SaveInfo
    {
        public string saveDisplayName { get; set; }
        public string planetId { get; set; }

        public bool unlockedSpaceTrading { get; set; }
        public bool unlockedOreExtrators { get; set; }
        public bool unlockedTeleporters { get; set; }
        public bool unlockedDrones { get; set; }
        public bool unlockedAutocrafter { get; set; }
        public bool unlockedEverything { get; set; }

        public bool freeCraft { get; set; }
        public bool preInterplanetarySave { get; set; }
        public bool randomizeMineables { get; set; }

        public float modifierTerraformationPace { get; set; }
        public float modifierPowerConsumption { get; set; }
        public float modifierGaugeDrain { get; set; }
        public float modifierMeteoOccurence { get; set; }
        public float modifierMultiplayerTerraformationFactor { get; set; }

        public bool modded { get; set; }
        public string version { get; set; }
        public string mode { get; set; }
        public string dyingConsequencesLabel { get; set; }
        public string startLocationLabel { get; set; }

        public long worldSeed { get; set; }
        public bool hasPlayedIntro { get; set; }
        public string gameStartLocation { get; set; }

        // Derived fields (optional)
        public bool IsModded => modded;
        public bool IsCreativeMode => mode?.Equals("Creative", StringComparison.OrdinalIgnoreCase) == true;
    }
}
