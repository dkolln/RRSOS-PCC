using RRSOS_PCC.Classes.Interfaces;
using RRSOS_PCC.Models;
using System.ComponentModel;
using System.Reflection.PortableExecutable;

namespace RRSOS_PCC.Enums
{
    

    public enum WorldObjectType
    {
        None = 0,

        // ============================
        // Equipment (1000)
        // ============================

        //PersonalEquipment
        [Description("Tool")]
        Tool = 1110,

        [Description("Gear")]
        Gear = 1120,

        [Description("Spacesuit")]
        Spacesuit = 1130,

        [Description("Transportation")]
        Transportation = 1140,

        //VehicleEquipment
        [Description("Vehicle Part")]
        Part = 1210,

        //MonitoringEquipment
        [Description("Screen")]
        Screen = 1310,


        // ============================
        // Resource (2000)
        // ============================

        //Ore = 2100
        [Description("Ore")]
        Ore = 2110,

        [Description("Alloy")]
        Alloy = 2120,

        [Description("Quartz")]
        Quartz = 2130,

        //Gas (2200)
        [Description("Gas Canister")]
        GasCanister = 2210,

        //Fluid (2300)
        [Description("Fluid Canister")]
        FluidCanister = 2310,

        //Rod (2400)
        [Description("Rod")]
        Rod = 2410,

        //FoodComponent (2500)
        [Description("Flour")]
        Flour = 2510,

        //PackagedResource (2600)
        [Description("Fertilizer")]
        Fertilizer = 2610,

        [Description("Explosive Powder")]
        ExplosivePowder = 2620,

        //Currency (2700)
        [Description("Tokens")]
        Tokens = 2710,

        // ============================
        // Biological (3000)
        // ============================
        //Petri Dish (3100)
            [Description("Mutagen")]
            Mutagen = 3110,

            [Description("Bacteria")]
            Bacteria = 3120,

            [Description("GeneticLab")]
            GeneticLab = 3210,

            [Description("DNALab")]
            DNALab = 3220,

            [Description("DNA")]
            DNA = 3230,

        //Larva (3300)
        [Description("Common Larva")]
            CommonLarva = 3310,

            [Description("Uncommon Larva")]
            UncommonLarva = 3320,

            [Description("Rare Larva")]
            RareLarva = 3330,

            [Description("Butterfly Larva")]
            ButterflyLarva = 3340,

            [Description("Bee Larva")]
            BeeLarva = 3350,

            [Description("Silk Worm")]
            SilkWorm = 3360,

        //Seed (3400)
        [Description("Flower Seed")]
            FlowerSeed = 3410,

            [Description("Tree Seed")]
            TreeSeed = 3420,

            [Description("Food Seed")]
            FoodSeed = 3430,

            [Description("Algae Seed")]
            AlgaeSeed = 3440,

        //Habitat (3500)
            [Description("Aquarium")]
            Aquarium = 3510,

            [Description("Animal Shelter")]
            AnimalShelter = 3520,

            [Description("Fish Farm")]
            FishFarm = 3530,

            [Description("Butterfly Farm")]
            ButterflyFarm = 3540,

            [Description("Incubator")]
            Incubator = 3550,

            [Description("Amphibian Farm")]
            AmphibianFarm = 3560,

        //Biological Component (3600)
        [Description("Plankton A")]
            PlanktonA = 3610,

            [Description("Plankton B")]
            PlanktonB = 3620,

            [Description("Plankton C")]
            PlanktonC = 3630,

            [Description("Plastic")]
            Plastic = 3640,

            [Description("Thread")]
            Thread = 3650,

            [Description("Tree Bark")]
            TreeBark = 3660,

        //Egg (3700)
        [Description("Frog Egg")]
            FrogEgg = 3710,

            [Description("Fish Egg")]
            FishEgg = 3720,

        // ============================
        // Consumable (4000)
        // ============================

        //-- Consumable 4000 -- Food 4100 --
        [Description("Vegetable")]
        Vegetable = 4110,

        [Description("Food")]
        Food = 4120,

        [Description("Water Bottle")]
        WaterBottle = 4310,

        //-- Consumable 4000 -- Blueprint 4500 --
        [Description("Blueprint")]
        Blueprint = 4510,

        [Description("SpecialBlueprint")]
        SpecialBlueprint = 4520,

        // --- Machine (5000) ---
        [Description("Drill")]
        Drill = 5110,

        [Description("Heater")]
        Heater = 5210,

        [Description("Tree Spreader")]
        TreeSpreader = 5310,

        [Description("Flower Spreader")]
        FlowerSpreader = 5320,

        [Description("Grass Spreader")]
        GrassSpreader = 5330,

        [Description("FluidExtractor")]
        FluidExtractor = 5410,

        [Description("LifeExtractor")]
        LifeExtractor = 5420,

        [Description("OreExtractor")]
        OreExtractor = 5430,

        [Description("Organic Extractor")]
        OrganicExtractor = 5440,

        [Description("GasExtractor")]
        GasExtractor = 5450,

        [Description("Food Grower")]
        FoodGrower = 5510,

        [Description("Food Farm")]
        FoodFarm = 5520,

        //-- Base Part 6000 -- 

        // -- Module 6100
        [Description("Wall")]
        Wall = 6110,
        
        [Description("Window")]
        Window = 6120,

        [Description("Door")]
        Door = 6130,

        [Description("Ladder")]
        Ladder = 6140,

        //Pod
        [Description("Pod")]
        Pod = 6210,

        [Description("Escape Pod")]
        EscapePod = 6220,

        [Description("Foundation")]
        Foundation = 6310,

        [Description("Arcade Machine")]
        ArcadeMachine = 6410,

        [Description("Library")]
        Library = 6420,

        [Description("Exercise Equipment")]
        ExerciseEquipment = 6430,

        [Description("Counter")]
        Counter = 6440,

        [Description("Shelves")]
        Shelves = 6450,

        [Description("Furniture")]
        Furniture = 6460,

        [Description("Appliance")]
        Appliance = 6470,

        [Description("Decoration")]
        Decoration = 6480,

        [Description("Poster")]
        Poster = 6490,

        [Description("Antenna")]
        Antenna = 6510,

        [Description("Teleporter")]
        Teleporter = 6520,
        
        [Description("Launch Platform")]
        LaunchPlatform = 6530,
         
        [Description("Trade Platform")]
        TradePlatform = 6540,

        [Description("Drone Station")]
        DroneStation = 6550,

        [Description("Vehicle Platform")]
        VehiclePlatform = 6560,

        [Description("Portal Generator")]
        PortalGenerator = 6570,

        //--Lab 6600
        [Description("Tree Farm")]
        TreeFarm = 6610,

        [Description("Genetic Manipulator")]
        GeneticManipulator = 6620,

        [Description("Biodome")]
        BioDome = 6630,

        [Description("Butterfly Dome")]
        ButterflyDome = 6640,

        [Description("Bio Lab")]
        BioLab = 6650,

        //--Crafter 6800
        [Description("Crafting Station")]
        CraftingStation = 6810,

        [Description("Vegetube")]
        Vegetube = 6820,

        [Description("Recycler")]
        Recycler = 6830,

        [Description("Kitchen")]
        Kitchen = 6840,

        [Description("Silk Generator")]
        SilkGenerator = 6850,

        //--Producer 6900
        [Description("Beehive")]
        Beehive = 6910,

        [Description("Power Generator")]
        PowerGenerator = 6920,

        [Description("Life Collector")]
        LifeCollector = 6930,

        [Description("Liquid Collector")]
        LiquidCollector = 6940,

        //--Component 7000

        //Circuit Board 7100
        [Description("Circuit Board")]
        CircuitBoard = 7110,

        //Cloth 7200
        [Description("Fabric")]
        Fabric = 7210,

        [Description("Smart Fabric")]
        SmartFabric = 7220,

        //Rocket Engine 7300
        [Description("Rocket Engine")]
        RocketEngine = 7330,

        //Modifier 8000
        [Description("Fuse")]
        Fuse = 8010,

        [Description("Optimizer")]
        Optimizer = 8030,

        [Description("Rocket")]
        Rocket = 8310,

        //--Container 9000

        //-- Base Container
        [Description("Container")]
        Container = 9110,

        //-- Portable Container
        [Description("Canister")]
        Canister = 9210,

        //-- World Container 9300
        [Description("Golden Container")]
        GoldenContainer = 9310,

        [Description("Vault")]
        Vault = 9320,

        [Description("Explosive")]
        Explosive = 9410,

        //-- Utility 11000 --
        [Description("Drone")]
        Drone = 11110,

        //-- Battery 11200 --
        [Description("Power Cell")]
        PowerCell = 11210,

        //-- Access Card 11300 --
        [Description("Access Card")]
        AccessCard = 11310,

        //-- Key 11400 --
        [Description("Warden Key")]
        WardenKey = 11410,

        //-- Wreck 12000 --

        //-- Wreck 12100 --
        [Description("Wreck")]
        Wreck = 12110,

        //-- Locked Door --
        [Description("Wreck Locked Door")]
        WreckEntryLocked = 12120,

        //-- Locked Door --
        [Description("Wreck Locked Generator")]
        WreckFusionGenerator = 12130,

        [Description("Wreck Debris")]
        WreckDebris = 12140,

        [Description("Wreck Boom Rocks")]
        WreckBoomRocks = 12150,



        [Description("Unknown")]
        Unknown = 9999
    }
}
