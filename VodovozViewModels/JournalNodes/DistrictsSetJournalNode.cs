using QS.Project.Journal;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.JournalNodes
{
    public class DistrictsSetJournalNode : JournalEntityNodeBase<DistrictsSet>
    {
        public string Name { get; set; }
        public Employee Creator { get; set; }
    }
}