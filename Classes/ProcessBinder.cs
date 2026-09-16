using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using RRSOS_PCC.Class;
using RRSOS_PCC.Components.Pages;
using RRSOS_PCC.Enums;
using RRSOS_PCC.Models;
using RRSOS_PCC.Services;
using RRSOS_PCC.ViewModels;
using System.Numerics;
using System.Resources;
using System.Text;

namespace RRSOS_PCC.Classes
{

    public static class ProcessBinder
    {
        private const float SIGN_RANGE = 4.0f;

        public static void BindContainers(SaveState state, WorldObjectClassifierService classifier)
        {
            if (state.WorldObjects == null || state.ContainerDetails == null)
                return;

            var worldById = state.WorldObjects.ToDictionary(wo => wo.id);
            var detailById = state.ContainerDetails.ToDictionary(cd => cd.id);

            // 1. Bind all world containers
            foreach (var container in state.Containers)
            {
                BindSingleContainer(container, detailById, worldById);
            }

            // 2. Bind backpack
            BindBackpack(state, detailById, worldById);

            // 3. Bind equipment
            BindEquipment(state, detailById, worldById);

            // 4. Bind vehicle
            BindVehicle(state, detailById, worldById);

            BindPlayer(state);

            // 5. Bind ore extractors
            BindExtractors(state, detailById, worldById, classifier);
        }

        private static void BindSingleContainer(
            Container container,
            Dictionary<long, ContainerDetail> detailById,
            Dictionary<long, WorldObject> worldById)
        {
            container.PrimaryItems.Clear();
            container.SecondaryItems.Clear();

            // Primary
            if (detailById.TryGetValue((int)container.liId, out var primary))
            {
                container.Hydrate();
                container.Capacity = primary.size ?? 0;

                foreach (var id in primary.ItemIds)
                {
                    if (worldById.TryGetValue(id, out var item))
                    {
                        container.PrimaryItems.Add(item);

                        item.Owner = new WorldObjectOwner
                        {
                            Type = WorldObjectOwnerType.Container,
                            Id = container.id
                        };
                    }
                }
            }

            Int32.TryParse(container.siIds, out var sId);

            // Secondary (rare)
            if (sId > 0 && detailById.TryGetValue(sId, out var secondary))
            {
                container.Capacity = secondary.size ?? 0;

                foreach (var id in secondary.GetItemIds())
                {
                    if (worldById.TryGetValue(id, out var item))
                    {
                        container.SecondaryItems.Add(item);

                        item.Owner = new WorldObjectOwner
                        {
                            Type = WorldObjectOwnerType.Container,
                            Id = container.id
                        };
                    }
                }
            }
        }

        //Bind the items in the player's backpack to the player's backpack.
        public static void BindBackpack(
            SaveState state,
            Dictionary<long, ContainerDetail> detailById,
            Dictionary<long, WorldObject> worldById)
        {
            if (state.Player == null)
                return;

            // Find the ContainerDetail for the player's backpack
            if (!state.Player.inventoryId.HasValue)
                return;

            if (!detailById.TryGetValue(state.Player.inventoryId.Value, out var detail))
                return;

            var backpack = state.Player.Backpack;

            // Hydrate backpack items
            backpack.Items.Clear();

            foreach (var id in detail.ItemIds)
            {
                if (worldById.TryGetValue(id, out var item))
                {
                    backpack.Items.Add(item);

                    // Assign ownership
                    item.Owner = new WorldObjectOwner
                    {
                        Type = WorldObjectOwnerType.Backpack,
                        Id = backpack.Id
                    };
                }
            }
        }

        //Bind the items in the player's equipment to the player's equipment.
        public static void BindEquipment(
            SaveState state,
            Dictionary<long, ContainerDetail> detailById,
            Dictionary<long, WorldObject> worldById)
        {
            if (state.Player == null)
                return;

            // Find the ContainerDetail that represents the player's equipment
            if (!state.Player.equipmentId.HasValue)
                return;

            if (!detailById.TryGetValue(state.Player.equipmentId.Value, out var detail))
                return;

            var equipment = state.Player.Equipment;

            // Hydrate equipment items
            equipment.Items.Clear();

            foreach (var id in detail.ItemIds)
            {
                if (worldById.TryGetValue(id, out var item))
                {
                    equipment.Items.Add(item);

                    // Assign ownership
                    item.Owner = new WorldObjectOwner
                    {
                        Type = WorldObjectOwnerType.Equipment,
                        Id = equipment.Id
                    };
                }
            }
        }

        public static void BindExtractors(
            SaveState state,
            Dictionary<long, ContainerDetail> detailById,
            Dictionary<long, WorldObject> worldById,
            WorldObjectClassifierService classifier)
        {
            // Always rebuild unified list
            state.Extractors.Clear();

            if (state.OreExtractors != null)
            {
                foreach (var extractor in state.OreExtractors)
                {
                    if (!detailById.TryGetValue(extractor.liId, out var detail))
                        continue;

                    var items = detail.ItemIds
                        .Where(id => worldById.TryGetValue(id, out _))
                        .Select(id => worldById[id])
                        .ToList();

                    var grouped = items
                        .GroupBy(i => i.gId)
                        .ToDictionary(g => g.Key, g => g.Count());

                    string primaryGid = extractor.liGrps?
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .FirstOrDefault() ?? "Unknown";

                    var d = classifier.GetDefinition(primaryGid);
                    
                    int primaryCount = grouped.TryGetValue(primaryGid, out var pc) ? pc : 0;

                    int totalCount = items.Count;
                    int capacity = detail.size ?? 0;
                    bool isFull = totalCount >= capacity;

                    extractor.Hydrate();

                    var position = new PositionViewModel(extractor.Position);
                    float distance = PCMath.GetDistance(position.Flat, state.Player.Position.Flat);

                    state.Extractors.Add(new ExtractorSummaryVM
                    {
                        Id = extractor.id,
                        Type = ExtractorType.Ore,
                        ProductGroup = d.Name,

                        PrimaryCount = primaryCount,
                        Count = totalCount,
                        Capacity = capacity,
                        IsFull = isFull,

                        Direction = PCMath.GetCompassDirection(position.Flat, state.Player.Position.Flat),

                        Position2D = position.Display2D,
                        Distance = distance,
                        GroupedContents = grouped
                    });
                }
            }

            if (state.WaterCollectors != null)
            {
                foreach (var wc in state.WaterCollectors)
                {
                    if (!detailById.TryGetValue(wc.liId, out var detail))
                        continue;

                    var items = detail.ItemIds
                        .Where(id => worldById.TryGetValue(id, out _))
                        .Select(id => worldById[id])
                        .ToList();

                    int totalCount = items.Count;
                    int capacity = detail.size ?? 0;
                    bool isFull = totalCount >= capacity;

                    wc.Hydrate();

                    var position = new PositionViewModel(wc.Position);
                    float distance = PCMath.GetDistance(position.Flat, state.Player.Position.Flat);

                    state.Extractors.Add(new ExtractorSummaryVM
                    {
                        Id = wc.id,
                        Type = ExtractorType.Water,
                        ProductGroup = "WaterBottle",

                        PrimaryCount = totalCount,
                        Count = totalCount,
                        Capacity = capacity,
                        IsFull = isFull,

                        Direction = PCMath.GetCompassDirection(position.Flat, state.Player.Position.Flat),

                        Position2D = position.Display2D,
                        Distance = distance,

                        GroupedContents = totalCount > 0
                            ? new Dictionary<string, int>
                            {
                                { "WaterBottle", totalCount }
                            }
                            : null
                                        });

                }
            }

            if (state.AlgaeGenerators != null)
            {
                foreach (var ag in state.AlgaeGenerators)
                {
                    Int64.TryParse(ag.siIds, out long result);

                    if (!detailById.TryGetValue(result, out var detail))
                        continue;

                    var items = detail.ItemIds
                        .Where(id => worldById.TryGetValue(id, out _))
                        .Select(id => worldById[id])
                        .ToList();

                    int totalCount = items.Count;
                    int capacity = detail.size ?? 0;

                    int readyCount = items.Count(i => i.grwth == 100);
                    int growingCount = items.Count(i => i.grwth < 100);

                    bool isFull = readyCount == capacity;

                    ag.Hydrate();

                    var position = new PositionViewModel(ag.Position);
                    float distance = PCMath.GetDistance(position.Flat, state.Player.Position.Flat);

                    state.Extractors.Add(new ExtractorSummaryVM
                    {
                        Id = ag.id,
                        Type = ExtractorType.Algae,
                        ProductGroup = "Algae",

                        PrimaryCount = readyCount,   // fully grown algae
                        Count = totalCount,          // total algae objects
                        Capacity = capacity,
                        IsFull = isFull,             // FULL = all grown

                        Direction = PCMath.GetCompassDirection(position.Flat, state.Player.Position.Flat),
                        Position2D = position.Display2D,
                        Distance = distance,

                        GroupedContents = new Dictionary<string, int>
                        {
                            { "Ready (100%)", readyCount },
                            { "Growing", growingCount }
                        }
                    });
                }
            }

            if (state.WaterLifeGenerators != null)
            {
                foreach (var wc in state.WaterLifeGenerators)
                {
                    if (!detailById.TryGetValue(wc.liId, out var detail))
                        continue;

                    var items = detail.ItemIds
                        .Where(id => worldById.TryGetValue(id, out _))
                        .Select(id => worldById[id])
                        .ToList();

                    int totalCount = items.Count;
                    int capacity = detail.size ?? 0;
                    bool isFull = totalCount >= capacity;

                    wc.Hydrate();

                    var position = new PositionViewModel(wc.Position);
                    float distance = PCMath.GetDistance(position.Flat, state.Player.Position.Flat);

                    state.Extractors.Add(new ExtractorSummaryVM
                    {
                        Id = wc.id,
                        Type = ExtractorType.WaterLife,
                        ProductGroup = "WaterLife",

                        PrimaryCount = totalCount,
                        Count = totalCount,
                        Capacity = capacity,
                        IsFull = isFull,

                        Direction = PCMath.GetCompassDirection(position.Flat, state.Player.Position.Flat),

                        Position2D = position.Display2D,
                        Distance = distance,

                        GroupedContents = totalCount > 0
                            ? new Dictionary<string, int>
                            {
                                { "WaterLife", totalCount }
                            }
                            : null
                    });

                }
            }

            if (state.Ecosystems != null)
            {
                foreach (var wc in state.Ecosystems)
                {
                    if (!detailById.TryGetValue(wc.liId, out var detail))
                        continue;

                    var items = detail.ItemIds
                        .Where(id => worldById.TryGetValue(id, out _))
                        .Select(id => worldById[id])
                        .ToList();

                    int totalCount = items.Count;
                    int capacity = detail.size ?? 0;
                    bool isFull = totalCount >= capacity;

                    wc.Hydrate();

                    var position = new PositionViewModel(wc.Position);
                    float distance = PCMath.GetDistance(position.Flat, state.Player.Position.Flat);

                    state.Extractors.Add(new ExtractorSummaryVM
                    {
                        Id = wc.id,
                        Type = ExtractorType.Larva,
                        ProductGroup = "Larvae",

                        PrimaryCount = totalCount,
                        Count = totalCount,
                        Capacity = capacity,
                        IsFull = isFull,

                        Direction = PCMath.GetCompassDirection(position.Flat, state.Player.Position.Flat),

                        Position2D = position.Display2D,
                        Distance = distance,

                        GroupedContents = totalCount > 0
                            ? new Dictionary<string, int>
                            {
                                { "Larvae", totalCount }
                            }
                            : null
                    });

                }
            }

            state.Extractors = state.Extractors
                .OrderBy(x => x.Distance)
                .ToList();
        }




        public static void BindPods(SaveState state)
        {
            if (state.Pods == null)
                return;

            foreach(var pod in state.Pods)
            {
                pod.Hydrate();
            }
        }

        public static void BindSigns(SaveState state)
        {
            if (state.Signs == null)
                return;

            foreach (var sign in state.Signs)
            {
                sign.Hydrate();
            }
        }

        public static void BindBases(SaveState state, BaseNamingService naming)
        {
            naming.CleanupOrphanedEntries(state.Bases);

            if (state.Pods == null || state.Pods.Count == 0)
                return;

            // Build base list
            state.Bases = state.Pods
                .Where(pod => pod.Panels.Any(panel => panel.HasDoor))
                .Select(pod => new Base { Pods = { pod } })
                .ToList();

            // Classify + set position
            foreach (var b in state.Bases)
            {
                var ep = b.EntrancePod;
                if (ep == null) continue;

                // 1. Classification & Position
                bool hasConnection = ep.Panels.Any(p => p.IsConnected);
                bool hasDoor = ep.Panels.Any(p => p.HasDoor);
                b.Type = (hasDoor && hasConnection) ? BaseType.Base : BaseType.Outpost;
                b.Position = ep.Position;

                // 2. Proximity Search: Find the sign text for THIS base
                // We check state.Signs (which you already hydrated in BindSigns)
                var nearbySignText = state.Signs?
                    .FirstOrDefault(s => Vector2.Distance(s.Position.Flat, b.Position.Flat) <= 6.0f)?
                    .text;

                // 3. Resolve Identity via the injected 'naming' instance
                // TIER 1: Sign Text > TIER 2: JSON > TIER 3: Procedural
                b.Name = naming.GetBaseName(b, nearbySignText);
            }
        }


        public static void BindPlayer(SaveState state)
        {
            if (state.Player == null)
                return;

            state.Player.Hydrate();
        }

        public static void BindVehicle(
            SaveState state,
            Dictionary<long, ContainerDetail> detailById,
            Dictionary<long, WorldObject> worldById)
        {
            if (state.Vehicle == null)
                return;

            var vehicle = state.Vehicle;

            //
            // 1. TRUNK
            //
            vehicle.TrunkItems.Clear();

            if (detailById.TryGetValue(vehicle.liId, out var trunkDetail))
            {
                foreach (var id in trunkDetail.ItemIds)
                {
                    if (worldById.TryGetValue(id, out var item))
                    {
                        vehicle.TrunkItems.Add(item);

                        item.Owner = new WorldObjectOwner
                        {
                            Type = WorldObjectOwnerType.Vehicle,
                            Id = vehicle.id
                        };
                    }
                }
            }

            //
            // 2. MODULES
            //
            vehicle.ModuleItems.Clear();

            // siIds is now an int (single module detail ID)
            if (vehicle.siIds != null && Int64.Parse(vehicle.siIds) > 0 && detailById.TryGetValue(Int64.Parse(vehicle.siIds), out var moduleDetail))
            {
                foreach (var id in moduleDetail.ItemIds)
                {
                    if (worldById.TryGetValue(id, out var item))
                    {
                        vehicle.ModuleItems.Add(item);

                        item.Owner = new WorldObjectOwner
                        {
                            Type = WorldObjectOwnerType.Vehicle,
                            Id = vehicle.id
                        };
                    }
                }
            }

            //
            // 3. Hydrate AFTER inventory is bound
            //
            vehicle.Hydrate();
        }


        //Always build last.
        public static void BindWorldObjects(SaveState state, WorldObjectClassifierService classifier)
        {
            foreach (var wo in state.WorldObjects)
            {
                wo.Hydrate();

                // 1. & 2. CLASSIFY & HYDRATE
                // We do this first so the 'wo' carries its Name and Category 
                // before we hand it to the Base.
                WorldObjectDefinition def = classifier.GetDefinition(wo.gId);

                wo.Type = def.Type;
                wo.Category = WorldObjectResolver.GetCategoryEnum(wo.Type);
                wo.Name = !string.IsNullOrWhiteSpace(wo.text) ? wo.text : def.Name;

                if(wo.pos != null)
                {
                    bool t = true;
                }    

                // 3. LINK
                var owner = PCMath.FindNearestBase(wo, state);

                if (owner == null) continue;

                wo.OwningBase = owner;

                // 4. ROUTE (The Resolved Logic)
                // No more static helper! The Base handles its own rolling up.
                owner.AddToContents(wo);
            }
        }

        public static void BindContainerOwner(SaveState state)
        {
            foreach (var container in state.Containers)
            {
                if(container.PrimaryItems.Count > 0)
                {
                    container.OwningBase = container.PrimaryItems[0].OwningBase;
                }
                else if (container.SecondaryItems.Count > 0)
                {
                    container.OwningBase = container.SecondaryItems[0].OwningBase;
                }
                else
                {
                    var nearestBase = PCMath.FindNearestBase(container, state);
                    if (nearestBase != null)
                        container.OwningBase = nearestBase;
                }
            }
        }
    }
}


