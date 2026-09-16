using RRSOS_PCC.Models;

namespace RRSOS_PCC.ViewModels
{
    public class PlayerViewModel
    {
        public Player Player { get; }
        public VitalsViewModel Vitals { get; }
        public PositionViewModel Position { get; }
        public RotationViewModel Rotation { get; }
        public BackpackViewModel Backpack { get; }
        public EquipmentViewModel Equipment { get; }

        public PlayerViewModel(Player player)
        {
            Player = player;
            Vitals = new VitalsViewModel(player);
            Position = new PositionViewModel(player.Position);
            Rotation = new RotationViewModel(player.Rotation);
            Backpack = new BackpackViewModel(player.Backpack);
            Equipment = new EquipmentViewModel(player.Equipment);
        }
    }

}
