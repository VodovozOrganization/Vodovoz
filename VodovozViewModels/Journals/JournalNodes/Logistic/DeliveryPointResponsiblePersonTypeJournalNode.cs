using QS.Project.Journal;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
    public class DeliveryPointResponsiblePersonTypeJournalNode : JournalEntityNodeBase<DeliveryPointResponsiblePersonType>
    {
        public override string Title => Name;
        public string Name { get; set; }
    }
}
