//Route json to appropriate area then bind using ProcessBinder.

using RRSOS_PCC.Models;
using RRSOS_PCC.Classes;
using System.Text.Json;

namespace RRSOS_PCC.Classes
{
    public static class ProcessRouter
    {
        public static void RouteLine(string json, SaveState state)
        {
            ProcessSingleton(json, state);
        }
        public static void ProcessSingleton(string json, SaveState state)
        {
            state.RawJson.Add(json);

            //Global Progression
            if (json.Contains("\"terraTokens\""))
            {
                state.GlobalProgression = JsonSerializer.Deserialize<GlobalProgression>(json)
                    ?? new GlobalProgression();
                return;
            }

            if(json.Contains("\"Sign\""))
            {
                state.Signs.Add(JsonSerializer.Deserialize<Sign>(json)
                    ?? new Sign());
                return;
            }

            // Ore Extractor
            if (json.Contains("\"OreExtractor"))
            {
                state.OreExtractors.Add(
                    JsonSerializer.Deserialize<OreExtractor>(json)
                    ?? new OreExtractor()
                );
                return;
            }

            // Water Collector
            if (json.Contains("\"WaterCollector"))
            {
                state.WaterCollectors.Add(
                    JsonSerializer.Deserialize<WaterCollector>(json)
                    ?? new WaterCollector()
                );
                return;
            }

            // Algae Generator
            if (json.Contains("\"AlgaeGenerator"))
            {
                state.AlgaeGenerators.Add(
                    JsonSerializer.Deserialize<AlgaeGenerator>(json)
                    ?? new AlgaeGenerator()
                );
                return;
            }

            // Water Life Generator
            if (json.Contains("\"WaterLifeCollector1"))
            {
                state.WaterLifeGenerators.Add(
                    JsonSerializer.Deserialize<WaterLifeGenerator>(json)
                    ?? new WaterLifeGenerator()
                );
                return;
            }

            //Ecosystem Generator
            if (json.Contains("\"Ecosystem"))
            {
                state.Ecosystems.Add(
                    JsonSerializer.Deserialize<Ecosystem>(json)
                    ?? new Ecosystem()
                );
                return;
            }


            //Planet Data
            if (json.Contains("\"unitOxygenLevel\""))
            {
                state.PlanetInfo = JsonSerializer.Deserialize<Planet>(json)
                    ?? new Planet();
                return;
            }

            //Player Data
            if (json.Contains("\"playerPosition\""))
            {
                state.Player = JsonSerializer.Deserialize<Player>(json)
                    ?? new Player();
                return;
            }

            //Vehicle Data
            if (json.Contains("\"VehicleTruck\""))
            {
                state.Vehicle = JsonSerializer.Deserialize<Vehicle>(json)
                    ?? new Vehicle();
                return;
            }

            //Container Data
            if (json.Contains("\"liId\"")|| json.Contains("\"siIds\""))
            {
                state.Containers.Add(JsonSerializer.Deserialize<Container>(json)
                    ?? new Container());

                return;
            }

            //Container Detail Information
            if (json.Contains("\"woIds\""))
            {
                state.ContainerDetails.Add(JsonSerializer.Deserialize<ContainerDetail>(json)
                    ?? new ContainerDetail());

                return;
            }

            //Base Information (All Pods)
            if (json.Contains("\"gId\":\"pod\"") || json.Contains("Escapepod"))
            {
                var pod = JsonSerializer.Deserialize<Pod>(json) ?? new Pod();

                pod.BuildPanels();

                state.Pods.Add(pod);
                return;
            }

            //Save Data
            if (json.Contains("\"saveDisplayName\""))
            {
                state.SaveInfo = JsonSerializer.Deserialize<SaveInfo>(json)
                    ?? new SaveInfo();

                return;
            }

            //Wrecks
            if (json.Contains("\"woIdsGenerated\""))
            {
                state.WreckInfo.Add(JsonSerializer.Deserialize<Wreck>(json)
                    ?? new Wreck());

                return;
            }

            //Planet Color Layers
            if (json.Contains("\"layerId\"") && json.Contains("\"colorBase\""))
            {
                state.PlanetLayers.Add(JsonSerializer.Deserialize<PlanetColorLayer>(json)
                    ?? new PlanetColorLayer());
                return;
            }

            //Save Diagnostics
            if (json.Contains("\"craftedObjects\""))
            {
                state.Diagnostics = JsonSerializer.Deserialize<SaveDiagnostics>(json)
                    ?? new SaveDiagnostics();
                return;
            }

            //Story Events
            if (json.Contains("StoryEvent"))
            {
                state.StoryEvents.Add(JsonSerializer.Deserialize<StoryEvent>(json)
                    ?? new StoryEvent());
                return;
            }

            //Message Events
            if (json.Contains("Message"))
            {
                state.Messages.Add(JsonSerializer.Deserialize<Message>(json)
                    ?? new Message());
                return;
            }

            //World Objects
            if (json.Contains("\"gId\"") && json.Contains("\"id\""))
            {
                state.WorldObjects.Add(JsonSerializer.Deserialize<WorldObject>(json)
                    ?? new WorldObject());

                return;
            }

            state.Unknowns.Add(json);

        }

    }
}
