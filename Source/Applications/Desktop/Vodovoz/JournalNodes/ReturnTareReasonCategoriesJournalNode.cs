using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.JournalNodes
{
    public class ReturnTareReasonCategoriesJournalNode : JournalEntityNodeBase<ReturnTareReasonCategory>
    {
		public override string Title => Name;
        public string Name { get; set; }
	}
}
