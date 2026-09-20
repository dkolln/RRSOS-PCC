using RRSOS_PCC.ViewModels;

namespace RRSOS_PCC.Classes
{
    /// <summary>
    /// Decides which base the "Base Contents" panel shows. Standing between two bases must not
    /// make the panel flip back and forth every refresh, so the current base is kept until
    /// another one is clearly closer.
    /// </summary>
    public static class BaseSelector
    {
        /// <param name="basesNearestFirst">Bases ordered nearest to the player first.</param>
        /// <param name="currentId">The base currently shown, if any.</param>
        /// <param name="switchThreshold">How much closer another base must be before switching.</param>
        public static long? Choose(IReadOnlyList<BaseViewModel>? basesNearestFirst, long? currentId, float switchThreshold)
        {
            if (basesNearestFirst == null || basesNearestFirst.Count == 0)
                return null;

            var closest = basesNearestFirst[0];

            if (currentId is not long id)
                return closest.Id;

            var current = basesNearestFirst.FirstOrDefault(b => b.Id == id);

            // The base we were showing is gone (different save, base removed)
            if (current == null)
                return closest.Id;

            if (closest.Id != current.Id && closest.Distance < current.Distance - switchThreshold)
                return closest.Id;

            return current.Id;
        }
    }
}
