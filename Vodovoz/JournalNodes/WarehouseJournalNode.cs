using QS.Project.Journal;
using Vodovoz.Domain.Store;

namespace Vodovoz.JournalNodes
{
    public class WarehouseJournalNode : JournalEntityNodeBase<Warehouse>
    {
        public override string Title => $"{Name}";

        public string Name { get; set; }
    }
}
