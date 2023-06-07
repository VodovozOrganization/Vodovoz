using QS.DomainModel.Entity;
using QS.Project.Journal;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
	public class PremiumTemplateJournalNode : JournalEntityNodeBase<PremiumTemplate>
	{
		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";
		public string Reason { get; set; }
		public decimal PremiumMoney { get; set; }
	}
}
