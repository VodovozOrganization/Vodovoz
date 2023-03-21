using QS.Project.Journal;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
	public class DeliveryPriceRuleJournalNode : JournalEntityNodeBase<DeliveryPriceRule>
	{
		public override string Title => $"{Name} {Water19LCount}";
		public int Water19LCount { get; set; }
		public int Water6LCount { get; set; }
		public int Water1500mlCount { get; set; }
		public int Water600mlCount { get; set; }
		public int Water500mlCount { get; set; }
		public string Name { get; set; }
		public decimal OrderMinSumEShopGoods { get; set; }

		public string Description => $"Если " +
			$"19л б. < {Water19LCount}шт. " +
			$"или 6л б. < {Water6LCount}шт. " +
			$"или 1500мл б. < {Water1500mlCount}шт. " +
			$"или 500мл б. < {Water500mlCount}шт.";
	}
}
