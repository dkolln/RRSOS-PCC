using RRSOS_PCC.Enums;
using RRSOS_PCC.ViewModels;
namespace RRSOS_PCC.Models;

public class SaveState
{
    public GlobalProgression GlobalProgression { get; set; } = new();

    public List<Sign> Signs { get; set; } = new();
    public Planet PlanetInfo { get; set; } = new();

    public Player Player { get; set; } = new();

    public Vehicle Vehicle { get; set; } = new();

    public List<WorldObject> WorldObjects { get; set; } = new();

    public List<Container> Containers { get; set; } = new();

    public List<ContainerDetail> ContainerDetails { get; set; } = new();

    public List<Base> Bases { get; set; } = new();

    public SaveInfo SaveInfo { get; set; } = new();

    public DateTime LoadedAt { get; set; } = DateTime.Now;

    public string? SavePath { get; set; }

    public List<string> RawJson { get; set; } = new();

    public List<Wreck> WreckInfo { get; set; } = new();

    public List<PlanetColorLayer> PlanetLayers { get; set; } = new();

    public SaveDiagnostics Diagnostics { get; set; } = new();

    public List<Message> Messages { get; set; } = new();

    public List<StoryEvent> StoryEvents { get; set; } = new();

    public List<Pod> Pods { get; set; } = new();

    public List<OreExtractor> OreExtractors { get; set; } = new();
    public List<WaterCollector> WaterCollectors { get; set; } = new();
    public List<AlgaeGenerator> AlgaeGenerators { get; set; } = new();
    public List<ExtractorSummaryVM> Extractors { get; set; } = new();
    public List<Ecosystem> Ecosystems { get; set; } = new();
    public List<WaterLifeGenerator> WaterLifeGenerators { get; set; } = new();

    public List<string> Unknowns { get; set; } = new();

}
