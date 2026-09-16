using System.ComponentModel;

namespace RRSOS_PCC.Enums
{
    public enum WorldObjectSubCategory
    {
        None = 0,

        // --- Equipment ---
        [Description("Personal Equipment")]
        PersonalEquipment = 1100,

        [Description("Vehicle Equipment")]
        VehicleEquipment = 1200,

        [Description("Monitoring Equipment")]
        MonitoringEquipment = 1300,

        // --- Resource ---
        [Description("Ore")]
        Ore = 2100,

        [Description("Gas")]
        Gas = 2200,

        [Description("Fluid")]
        Fluid = 2300,

        [Description("Rod")]
        Rod = 2400,

        [Description("Food Component")]
        FoodComponent = 2500,

        [Description("Packaged Resource")]
        PackagedResource = 2600,

        [Description("Currency")]
        Currency = 2700,


        // --- Biological ---
        [Description("Petri Dish")]
        PetriDish = 3100,

        [Description("Genetic")]
        Genetic = 3200,

        [Description("Larva")]
        BaseLarva = 3300,

        [Description("Seed")]
        Seed = 3400,

        [Description("Habitat")]
        Habitat = 3500,

        [Description("Biological Component")]
        BioComponent = 3600,

        [Description("Egg")]
        Egg = 3700,

        // --- Consumable ---
        [Description("Food")]
        Food = 4100,

        [Description("Water Bottle")]
        WaterBottle = 4300,

        [Description("Oxygen Capsule")]
        OxygenCapsule = 4400,

        [Description("Blueprint")]
        Blueprint = 4500,

        // --- Machine ---
        [Description("Drill")]
        Drill = 5100,

        [Description("Heater")]
        Heater = 5200,

        [Description("Spreader")]
        Spreader = 5300,

        [Description("Collector")]
        Collector = 5400,

        [Description("Grower")]
        Grower = 5500,


        // --- Base Part ---

        [Description("Module")]
        Module = 6100,

        [Description("Pod")]
        Pod = 6200,

        [Description("Foundation")]
        Foundation = 6300,

        [Description("Decoration")]
        Decoration = 6400,

        [Description("Platform")]
        Platform = 6500,

        [Description("Lab")]
        Lab = 6600,

        [Description("Screen")]
        Screen = 6700,

        [Description("Crafter")]
        Crafter = 6800,

        [Description("Producer")]
        Producer = 6900,

        // --- Component ---
        [Description("Circuit Board")]
        CircuitBoard = 7100,

        [Description("Cloth")]
        Cloth = 7200,

        [Description("Rocket Engine")]
        RocketEngine = 7300,

        // --- Modifier ---
        [Description("Fuse")]
        Fuse = 8100,

        [Description("Enhancer")]
        Enhancer = 8200,

        [Description("Rocket")]
        Rocket = 8300,

        // --- Container ---
        [Description("Base Container")]
        BaseContainer = 9100,

        [Description("Portable Container")]
        PortableContainer = 9200,

        [Description("World Container")]
        WorldContainer = 9300,

        [Description("Spicy Container")]
        SpicyContainer = 9400,

        // --- World Marker ---
        [Description("Beacon")]
        Beacon = 10100,

        [Description("Sign")]
        Sign = 10200,

        // --- Utility Items ---
        [Description("Drone")]
        Drone = 11100,

        [Description("Battery")]
        Battery = 11200,

        [Description("Access Card")]
        AccessCard = 11300,

        [Description("Key")]
        Key = 11400,

        [Description("Wreck")]
        Wreck = 12100,


        // --- Unknown ---

        [Description("Unknown")]
        Unknown = 9999
    }
}
