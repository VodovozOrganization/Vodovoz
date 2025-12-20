using QS.Project.Journal;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Goods
{
	public class InventoryInstancesJournalNode : JournalEntityNodeBase<InventoryNomenclatureInstance>
	{
		public override string Title => $"{NomenclatureName} {InventoryNumber}";
		public int NomenclatureId { get; set; }
		public string NomenclatureName { get; set; }
		public string InventoryNumber { get; set; }
		public bool IsUsed { get; set; }
		public string GetInventoryNumber => InventoryNomenclatureInstance.GetInventoryNumberString(IsUsed, InventoryNumber);
	}
	
	public class InventoryInstancesStockJournalNode : InventoryInstancesJournalNode
	{
		public decimal Balance { get; set; }
	}
}
