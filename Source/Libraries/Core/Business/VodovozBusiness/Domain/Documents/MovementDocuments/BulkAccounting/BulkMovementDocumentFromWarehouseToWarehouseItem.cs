using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.MovementDocuments.BulkAccounting
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения со склада на склад(объемный учет)",
		Nominative = "строка перемещения со склада на склад(объемный учет)")]
	public class BulkMovementDocumentFromWarehouseToWarehouseItem : BulkMovementDocumentToWarehouseItem
	{
		public override MovementDocumentItemType MovementDocumentItemType =>
			MovementDocumentItemType.BulkMovementDocumentFromWarehouseToWarehouseItem;
		
		public virtual WarehouseBulkGoodsAccountingOperation WriteOffWarehouseBulkGoodsAccountingOperation
		{
			get => WriteOffOperation as WarehouseBulkGoodsAccountingOperation;
			set => WriteOffOperation = value;
		}
		
		protected override void CreateOrUpdateWriteOffOperation()
		{
			if(WriteOffOperation is null)
			{
				WriteOffWarehouseBulkGoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation();
			}

			WriteOffWarehouseBulkGoodsAccountingOperation.Warehouse = Document.FromWarehouse;
		}
	}
}
