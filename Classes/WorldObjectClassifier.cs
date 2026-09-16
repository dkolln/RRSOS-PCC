using RRSOS_PCC.Enums;

namespace RRSOS_PCC.Classes
{
    public static class WorldObjectClassifier
    {
        public static WorldObjectType Classify(string gId)
        {
            var entry = WorldObjectDataService.GetEntry(gId);

            if (entry != null)
                return entry.Type;

            // Not found → record it
            UnknownTypeRegistry.Register(gId);
            return WorldObjectType.Unknown;
        }

    }

}
