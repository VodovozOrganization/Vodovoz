using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения на склад(объемный учет)",
		Nominative = "строка перемещения на склад(объемный учет)")]
	public abstract class BulkMovementDocumentToWarehouseItem : BulkMovementDocumentItem
	{
		public WarehouseBulkGoodsAccountingOperation IncomeWarehouseBulkGoodsAccountingOperation
		{
			get => IncomeOperation as WarehouseBulkGoodsAccountingOperation;
			set => IncomeOperation = value;
		}
		
		protected override void CreateIncomeOperation()
		{
			if(IncomeOperation is null)
			{
				IncomeWarehouseBulkGoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation();
			}
		}
		
		protected override void FillIncomeStorage()
		{
			IncomeWarehouseBulkGoodsAccountingOperation.Warehouse = Document.ToWarehouse;
		}
	}
}
