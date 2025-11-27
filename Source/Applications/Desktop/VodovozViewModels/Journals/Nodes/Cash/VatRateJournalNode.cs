using QS.Project.Journal;
using Vodovoz.Core.Domain.Cash;

namespace Vodovoz.ViewModels.Journals.Nodes.Cash
{
	public class VatRateJournalNode : JournalEntityNodeBase<VatRate>
	{
		public override string Title => VatRateValue;

		public string VatRateValue { get; set; }
	}
}
