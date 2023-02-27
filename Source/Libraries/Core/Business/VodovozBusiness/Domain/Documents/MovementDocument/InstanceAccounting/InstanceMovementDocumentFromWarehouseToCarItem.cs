using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения со склада на автомобиль(экземплярный учет)",
		Nominative = "строка перемещения со склада на автомобиль(экземплярный учет)")]
	public class InstanceMovementDocumentFromWarehouseToCarItem : InstanceMovementDocumentToCarItem
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
