using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения с автомобиля на автомобиль(экземплярный учет)",
		Nominative = "строка перемещения с автомобиля на автомобиль(экземплярный учет)")]
	public class InstanceMovementDocumentFromToCarItem : InstanceMovementDocumentToCarItem
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
