using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения на автомобиль(объемный учет)",
		Nominative = "строка перемещения на автомобиль(объемный учет)")]
	public abstract class BulkMovementDocumentToCarItem : BulkMovementDocumentItem
	{
		public CarBulkGoodsAccountingOperation IncomeCarBulkGoodsAccountingOperation
		{
			get => IncomeOperation as CarBulkGoodsAccountingOperation;
			set => IncomeOperation = value;
		}

		protected override void CreateIncomeOperation()
		{
			if(IncomeOperation is null)
			{
				IncomeCarBulkGoodsAccountingOperation = new CarBulkGoodsAccountingOperation();
			}
		}

		protected override void FillIncomeStorage()
		{
			IncomeCarBulkGoodsAccountingOperation.Car = Document.ToCar;
		}
	}
}
