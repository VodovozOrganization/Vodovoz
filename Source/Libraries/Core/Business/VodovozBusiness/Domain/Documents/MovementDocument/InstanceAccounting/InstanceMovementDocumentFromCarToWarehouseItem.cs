using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения с автомобиля на склад(экземплярный учет)",
		Nominative = "строка перемещения с автомобиля на склад(экземплярный учет)")]
	public class InstanceMovementDocumentFromCarToWarehouseItem : InstanceMovementDocumentToWarehouseItem
	{
		public CarInstanceGoodsAccountingOperation WriteOffCarInstanceGoodsAccountingOperation
		{
			get => WriteOffOperation as CarInstanceGoodsAccountingOperation;
			set => WriteOffOperation = value;
		}
		
		protected override void CreateWriteOffOperation()
		{
			if(WriteOffOperation is null)
			{
				WriteOffCarInstanceGoodsAccountingOperation = new CarInstanceGoodsAccountingOperation();
			}
		}
		
		protected override void FillWriteOffStorage()
		{
			WriteOffCarInstanceGoodsAccountingOperation.Car = Document.FromCar;
		}
	}
}
