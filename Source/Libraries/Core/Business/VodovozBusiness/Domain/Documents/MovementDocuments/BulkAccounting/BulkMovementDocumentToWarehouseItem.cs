using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.MovementDocuments.BulkAccounting
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения на склад(объемный учет)",
		Nominative = "строка перемещения на склад(объемный учет)")]
	public abstract class BulkMovementDocumentToWarehouseItem : BulkMovementDocumentItem
	{
		public virtual WarehouseBulkGoodsAccountingOperation IncomeWarehouseBulkGoodsAccountingOperation
		{
			get => IncomeOperation as WarehouseBulkGoodsAccountingOperation;
			set => IncomeOperation = value;
		}
		
		protected override void CreateOrUpdateIncomeOperation()
		{
			if(IncomeOperation is null)
			{
				IncomeWarehouseBulkGoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation();
			}

			IncomeWarehouseBulkGoodsAccountingOperation.Warehouse = Document.ToWarehouse;
		}
	}
}
