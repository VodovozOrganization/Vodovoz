using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения с автомобиля на сотрудника(объемный учет)",
		Nominative = "строка перемещения с автомобиля на сотрудника(объемный учет)")]
	public class BulkMovementDocumentFromCarToEmployeeItem : BulkMovementDocumentToEmployeeItem
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
