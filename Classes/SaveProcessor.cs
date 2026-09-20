using RRSOS_PCC.Models;
using RRSOS_PCC.Services;

namespace RRSOS_PCC.Classes
{
    public class SaveProcessor
    {
        private readonly SaveService _save;
        private readonly BaseNamingService _naming;
        private readonly WorldObjectClassifierService _objectService;

        public SaveProcessor(SaveService save, BaseNamingService naming, WorldObjectClassifierService objectService)
        {
            _save = save;
            _naming = naming;
            _objectService = objectService;
        }

        public SaveState Process(string jsonString)
        {
            var state = new SaveState();

            // '@' separates blocks, but only outside JSON strings.
            foreach (var part in SaveSplitter.Split(jsonString, '@'))
            {
                _save.ProcessBlock(new JsonBlock(part), state);
            }

            _save.BindBlocks(state, _naming, _objectService);

            return state;
        }
    }
}
