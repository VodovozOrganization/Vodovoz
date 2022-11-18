using QS.Project.Journal;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
	public class PremiumTemplateJournalNode : JournalEntityNodeBase<PremiumTemplate>
	{
		public string Reason { get; set; }
		public decimal PremiumMoney { get; set; }
	}
}
