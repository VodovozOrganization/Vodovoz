using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.MovementDocuments.BulkAccounting
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения с автомобиля на автомобиль(объемный учет)",
		Nominative = "строка перемещения с автомобиля на автомобиль(объемный учет)")]
	public class BulkMovementDocumentFromCarToCarItem : BulkMovementDocumentToCarItem
	{
		public override MovementDocumentItemType MovementDocumentItemType => MovementDocumentItemType.BulkMovementDocumentFromCarToCarItem;
		
		public virtual CarBulkGoodsAccountingOperation WriteOffCarBulkGoodsAccountingOperation
		{
			get => WriteOffOperation as CarBulkGoodsAccountingOperation;
			set => WriteOffOperation = value;
		}
		
		protected override void CreateOrUpdateWriteOffOperation()
		{
			if(WriteOffOperation is null)
			{
				WriteOffCarBulkGoodsAccountingOperation = new CarBulkGoodsAccountingOperation();
			}

			WriteOffCarBulkGoodsAccountingOperation.Car = Document.FromCar;
		}
	}
}
