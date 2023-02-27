using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения со склада на склад(объемный учет)",
		Nominative = "строка перемещения со склада на склад(объемный учет)")]
	public class BulkMovementDocumentFromToWarehouseItem : BulkMovementDocumentToWarehouseItem
	{
		public WarehouseBulkGoodsAccountingOperation WriteOffWarehouseBulkGoodsAccountingOperation
		{
			get => WriteOffOperation as WarehouseBulkGoodsAccountingOperation;
			set => WriteOffOperation = value;
		}
		
		protected override void CreateWriteOffOperation()
		{
			if(WriteOffOperation is null)
			{
				WriteOffWarehouseBulkGoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation();
			}
		}
		
		protected override void FillWriteOffStorage()
		{
			WriteOffWarehouseBulkGoodsAccountingOperation.Warehouse = Document.FromWarehouse;
		}
	}
}
