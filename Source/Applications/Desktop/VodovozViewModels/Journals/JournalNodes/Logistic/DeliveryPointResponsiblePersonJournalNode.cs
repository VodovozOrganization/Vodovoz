using QS.Project.Journal;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
    public class DeliveryPointResponsiblePersonJournalNode : JournalEntityNodeBase<DeliveryPointResponsiblePerson>
    {
        public override string Title => Employee.FullName;
        public DeliveryPointResponsiblePersonType DeliveryPointResponsiblePersonType { get; set; }
        public Employee Employee { get; set; }
        public string Phone { get; set; }
    }
}
