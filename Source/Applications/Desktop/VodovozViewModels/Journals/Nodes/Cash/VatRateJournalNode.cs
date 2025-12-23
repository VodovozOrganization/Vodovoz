using QS.Project.Journal;
using Vodovoz.Core.Domain.Cash;

namespace Vodovoz.ViewModels.Journals.Nodes.Cash
{
	public class VatRateJournalNode : JournalEntityNodeBase<VatRate>
	{
		public override string Title => VatRateValue == 0 ? "Без НДС" : $"{VatRateValue}%";

		public decimal VatRateValue { get; set; }
	}
}
