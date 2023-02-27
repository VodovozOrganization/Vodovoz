using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения на склад(экземплярный учет)",
		Nominative = "строка перемещения на склад(экземплярный учет)")]
	public abstract class InstanceMovementDocumentToWarehouseItem : InstanceMovementDocumentItem
	{
		public WarehouseInstanceGoodsAccountingOperation IncomeWarehouseInstanceGoodsAccountingOperation
		{
			get => IncomeOperation as WarehouseInstanceGoodsAccountingOperation;
			set => IncomeOperation = value;
		}
		
		protected override void CreateIncomeOperation()
		{
			if(IncomeOperation is null)
			{
				IncomeWarehouseInstanceGoodsAccountingOperation = new WarehouseInstanceGoodsAccountingOperation();
			}
		}
		
		protected override void FillIncomeStorage()
		{
			IncomeWarehouseInstanceGoodsAccountingOperation.Warehouse = Document.ToWarehouse;
		}
	}
}
