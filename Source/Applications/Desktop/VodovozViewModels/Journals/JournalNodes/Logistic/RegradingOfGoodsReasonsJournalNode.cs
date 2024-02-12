using QS.Project.Journal;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class RegradingOfGoodsReasonsJournalNode : JournalEntityNodeBase<RegradingOfGoodsReason>
	{
		public string Name { get; set; }
		public override string Title => Name;
	}
}
