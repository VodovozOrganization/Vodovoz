using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.MovementDocuments.InstanceAccounting
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения на автомобиль(экземплярный учет)",
		Nominative = "строка перемещения на автомобиль(экземплярный учет)")]
	public abstract class InstanceMovementDocumentToCarItem : InstanceMovementDocumentItem
	{
		public virtual CarInstanceGoodsAccountingOperation IncomeCarInstanceGoodsAccountingOperation
		{
			get => IncomeOperation as CarInstanceGoodsAccountingOperation;
			set => IncomeOperation = value;
		}

		protected override void CreateOrUpdateIncomeOperation()
		{
			if(IncomeOperation is null)
			{
				IncomeCarInstanceGoodsAccountingOperation = new CarInstanceGoodsAccountingOperation();
			}

			IncomeCarInstanceGoodsAccountingOperation.InventoryNomenclatureInstance = InventoryNomenclatureInstance;
			IncomeCarInstanceGoodsAccountingOperation.Car = Document.ToCar;
		}
	}
}
