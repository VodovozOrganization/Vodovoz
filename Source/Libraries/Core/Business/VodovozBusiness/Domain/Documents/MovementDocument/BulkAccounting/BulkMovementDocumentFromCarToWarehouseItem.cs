using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения с автомобиля на склад(объемный учет)",
		Nominative = "строка перемещения с автомобиля на склад(объемный учет)")]
	public class BulkMovementDocumentFromCarToWarehouseItem : BulkMovementDocumentToWarehouseItem
	{
		public CarBulkGoodsAccountingOperation WriteOffCarBulkGoodsAccountingOperation
		{
			get => WriteOffOperation as CarBulkGoodsAccountingOperation;
			set => WriteOffOperation = value;
		}
		
		protected override void CreateWriteOffOperation()
		{
			if(WriteOffOperation is null)
			{
				WriteOffCarBulkGoodsAccountingOperation = new CarBulkGoodsAccountingOperation();
			}
		}
		
		protected override void FillWriteOffStorage()
		{
			WriteOffCarBulkGoodsAccountingOperation.Car = Document.FromCar;
		}
	}
}
