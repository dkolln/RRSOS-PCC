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

        // Health / Thirst / Toxic status
        public bool HealthWarning => HealthPct < 0.50f;
        public bool HealthCritical => HealthPct < 0.20f;

        public bool ThirstWarning => ThirstPct < 0.50f;
        public bool ThirstCritical => ThirstPct < 0.20f;

        public bool ToxicWarning => ToxicPct > 0.50f;
        public bool ToxicCritical => ToxicPct > 0.80f;

        //Oxygen status
        public bool OxygenWarning => OxygenPct < 0.50f;
        public bool OxygenCritical => OxygenPct < 0.20f;
    }
}
