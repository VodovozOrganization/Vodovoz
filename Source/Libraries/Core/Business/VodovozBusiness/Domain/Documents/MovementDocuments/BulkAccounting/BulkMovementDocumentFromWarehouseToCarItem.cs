using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Operations;

namespace Vodovoz.Domain.Documents.MovementDocuments.BulkAccounting
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения со склада на автомобиль(объемный учет)",
		Nominative = "строка перемещения со склада на автомобиль(объемный учет)")]
	public class BulkMovementDocumentFromWarehouseToCarItem : BulkMovementDocumentToCarItem
	{
		public override MovementDocumentItemType MovementDocumentItemType =>
			MovementDocumentItemType.BulkMovementDocumentFromWarehouseToCarItem;
		
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
