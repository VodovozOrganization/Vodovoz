using QS.Project.Journal;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class RegradingOfGoodsReasonsJournalNode : JournalEntityNodeBase<RegradingOfGoodsReason>
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public override string Title => Name;
	}
}
