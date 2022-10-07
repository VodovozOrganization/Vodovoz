using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.JournalNodes
{
    public class ReturnTareReasonCategoriesJournalNode : JournalEntityNodeBase<ReturnTareReasonCategory>
    {
        public string Name { get; set; }
    }
}