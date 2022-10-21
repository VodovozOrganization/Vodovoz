using QS.Project.Journal;
using Vodovoz.Domain.Store;

namespace Vodovoz.JournalNodes
{
    public class MovementWagonJournalNode : JournalEntityNodeBase<MovementWagon>
    {
        public override string Title => Name;

        public string Name { get; set; }
    }
}
