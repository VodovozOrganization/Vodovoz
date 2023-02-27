using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения со склада на автомобиль(объемный учет)",
		Nominative = "строка перемещения со склада на автомобиль(объемный учет)")]
	public class BulkMovementDocumentFromWarehouseToCarItem : BulkMovementDocumentToCarItem
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
