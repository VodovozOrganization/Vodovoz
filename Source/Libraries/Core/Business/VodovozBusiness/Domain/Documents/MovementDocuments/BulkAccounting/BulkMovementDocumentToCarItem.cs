using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.MovementDocuments.BulkAccounting
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения на автомобиль(объемный учет)",
		Nominative = "строка перемещения на автомобиль(объемный учет)")]
	public abstract class BulkMovementDocumentToCarItem : BulkMovementDocumentItem
	{
		public virtual CarBulkGoodsAccountingOperation IncomeCarBulkGoodsAccountingOperation
		{
			get => IncomeOperation as CarBulkGoodsAccountingOperation;
			set => IncomeOperation = value;
		}

		protected override void CreateOrUpdateIncomeOperation()
		{
			if(IncomeOperation is null)
			{
				IncomeCarBulkGoodsAccountingOperation = new CarBulkGoodsAccountingOperation();
			}

			IncomeCarBulkGoodsAccountingOperation.Car = Document.ToCar;
		}
	}
}
