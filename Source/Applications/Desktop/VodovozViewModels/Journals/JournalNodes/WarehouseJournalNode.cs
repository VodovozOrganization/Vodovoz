using QS.DomainModel.Entity;
using QS.Project.Journal;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
    public class WarehouseJournalNode : JournalEntityNodeBase<Warehouse>, INamedDomainObject
    {
        public override string Title => Name;
        public string Name { get; set; }
    }
}