using RRSOS_PCC.Classes;
using RRSOS_PCC.Models;
using System.Numerics;

namespace RRSOS_PCC.ViewModels
{
    public class VitalsViewModel
    {
        private readonly Player _player;
        private readonly float _oxygenstat;
        public VitalsViewModel(Player player)
        {
            _player = player;
            string oxygenitem = player.Equipment.Items.Count > 0 && player.Equipment.Items.Any(x => x.gId.Contains("OxygenTank")) ? player.Equipment.Items.FirstOrDefault(x => x.gId.Contains("OxygenTank")).gId : "OxygenTank1";
            _oxygenstat = OxygenTankStats.MaxOxygen.FirstOrDefault(g => g.Key == oxygenitem).Value;
        }

        //Vitals
        public float PlayerGaugeOxygen => (float)((_player.playerGaugeOxygen ?? 0));
        public float PlayerGaugeThirst => (float)((_player.playerGaugeThirst ?? 0));
        public float PlayerGaugeHealth => (float)((_player.playerGaugeHealth ?? 0));
        public float PlayerGaugeToxic => (float)((_player.playerGaugeToxic ?? 0));

        // Vitals as Percentages
        public float HealthPct => (float)((_player.playerGaugeHealth ?? 0) / 100.0);
        public float ThirstPct => (float)((_player.playerGaugeThirst ?? 0) / 100.0);
        public float ToxicPct  => (float)((_player.playerGaugeToxic ?? 0) / 100.0);

        public float OxygenPct => (float)((_player.playerGaugeOxygen ?? 0) / _oxygenstat);

        /// <summary>What a full tank holds, in the game's own oxygen units; 0 if the equipped tank is not in the table.</summary>
        public float OxygenCapacity => _oxygenstat;

        // Status thresholds, shared with the dials so their colour zones always match the warnings.
        // Oxygen, health and thirst are bad when low; toxicity is bad when high.
        public const float LowWarnBelow = 0.50f;
        public const float LowCritBelow = 0.20f;
        public const float HighWarnAbove = 0.50f;
        public const float HighCritAbove = 0.80f;

        // Health / Thirst / Toxic status
        public bool HealthWarning => HealthPct < LowWarnBelow;
        public bool HealthCritical => HealthPct < LowCritBelow;

        public bool ThirstWarning => ThirstPct < LowWarnBelow;
        public bool ThirstCritical => ThirstPct < LowCritBelow;

        public bool ToxicWarning => ToxicPct > HighWarnAbove;
        public bool ToxicCritical => ToxicPct > HighCritAbove;

        //Oxygen status
        public bool OxygenWarning => OxygenPct < LowWarnBelow;
        public bool OxygenCritical => OxygenPct < LowCritBelow;
    }
}
