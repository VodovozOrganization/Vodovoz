using QS.Project.Journal;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Goods
{
	public class InventoryInstancesJournalNode : JournalEntityNodeBase<InventoryNomenclatureInstance>
	{
		public override string Title => $"{NomenclatureName} {InventoryNumber}";
		public int NomenclatureId { get; set; }
		public string NomenclatureName { get; set; }
		public string InventoryNumber { get; set; }
	}
	
	public class InventoryInstancesStockJournalNode : InventoryInstancesJournalNode
	{
		public decimal Balance { get; set; }
	}
}
