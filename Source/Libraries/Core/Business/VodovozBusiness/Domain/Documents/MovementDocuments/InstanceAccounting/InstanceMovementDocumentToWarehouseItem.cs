using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.MovementDocuments.InstanceAccounting
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения на склад(экземплярный учет)",
		Nominative = "строка перемещения на склад(экземплярный учет)")]
	public abstract class InstanceMovementDocumentToWarehouseItem : InstanceMovementDocumentItem
	{
		public virtual WarehouseInstanceGoodsAccountingOperation IncomeWarehouseInstanceGoodsAccountingOperation
		{
			get => IncomeOperation as WarehouseInstanceGoodsAccountingOperation;
			set => IncomeOperation = value;
		}
		
		protected override void CreateOrUpdateIncomeOperation()
		{
			if(IncomeOperation is null)
			{
				IncomeWarehouseInstanceGoodsAccountingOperation = new WarehouseInstanceGoodsAccountingOperation();
			}

			IncomeWarehouseInstanceGoodsAccountingOperation.InventoryNomenclatureInstance = InventoryNomenclatureInstance;
			IncomeWarehouseInstanceGoodsAccountingOperation.Warehouse = Document.ToWarehouse;
		}
	}
}
