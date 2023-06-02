using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.MovementDocuments.InstanceAccounting
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения со склада на автомобиль(экземплярный учет)",
		Nominative = "строка перемещения со склада на автомобиль(экземплярный учет)")]
	public class InstanceMovementDocumentFromWarehouseToCarItem : InstanceMovementDocumentToCarItem
	{
		public override MovementDocumentItemType MovementDocumentItemType =>
			MovementDocumentItemType.InstanceMovementDocumentFromWarehouseToCarItem;
		
		public virtual WarehouseInstanceGoodsAccountingOperation WriteOffWarehouseInstanceGoodsAccountingOperation
		{
			get => WriteOffOperation as WarehouseInstanceGoodsAccountingOperation;
			set => WriteOffOperation = value;
		}
		
		protected override void CreateOrUpdateWriteOffOperation()
		{
			if(WriteOffOperation is null)
			{
				WriteOffWarehouseInstanceGoodsAccountingOperation = new WarehouseInstanceGoodsAccountingOperation();
			}

			WriteOffWarehouseInstanceGoodsAccountingOperation.InventoryNomenclatureInstance = InventoryNomenclatureInstance;
			WriteOffWarehouseInstanceGoodsAccountingOperation.Warehouse = Document.FromWarehouse;
		}
	}
}
