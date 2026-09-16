using RRSOS_PCC.Models;
using RRSOS_PCC.Services;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks.Dataflow;

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

            var blocks = jsonString
                .Split('@', StringSplitOptions.RemoveEmptyEntries)
                .Select(b => new JsonBlock(b))
                .ToList();

            foreach (var block in blocks)
            {
                _save.ProcessBlock(block, state);
            }

            _save.BindBlocks(state, _naming, _objectService);

            return state;
        }
    }
}
