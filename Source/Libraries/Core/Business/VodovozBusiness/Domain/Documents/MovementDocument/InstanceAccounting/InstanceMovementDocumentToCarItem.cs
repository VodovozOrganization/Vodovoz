using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения на автомобиль(экземплярный учет)",
		Nominative = "строка перемещения на автомобиль(экземплярный учет)")]
	public abstract class InstanceMovementDocumentToCarItem : InstanceMovementDocumentItem
	{
		public CarInstanceGoodsAccountingOperation IncomeCarInstanceGoodsAccountingOperation
		{
			get => IncomeOperation as CarInstanceGoodsAccountingOperation;
			set => IncomeOperation = value;
		}

		protected override void CreateIncomeOperation()
		{
			if(IncomeOperation is null)
			{
				IncomeCarInstanceGoodsAccountingOperation = new CarInstanceGoodsAccountingOperation();
			}
		}

		protected override void FillIncomeStorage()
		{
			IncomeCarInstanceGoodsAccountingOperation.Car = Document.ToCar;
		}
	}
}
