using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения со склада на склад(экземплярный учет)",
		Nominative = "строка перемещения со склада на склад(экземплярный учет)")]
	public class InstanceMovementDocumentFromToWarehouseItem : InstanceMovementDocumentToWarehouseItem
	{
		public WarehouseInstanceGoodsAccountingOperation WriteOffWarehouseInstanceGoodsAccountingOperation
		{
			get => WriteOffOperation as WarehouseInstanceGoodsAccountingOperation;
			set => WriteOffOperation = value;
		}
		
		protected override void CreateWriteOffOperation()
		{
			if(WriteOffOperation is null)
			{
				WriteOffWarehouseInstanceGoodsAccountingOperation = new WarehouseInstanceGoodsAccountingOperation();
			}
		}
		
		protected override void FillWriteOffStorage()
		{
			WriteOffWarehouseInstanceGoodsAccountingOperation.Warehouse = Document.FromWarehouse;
		}
	}
}
