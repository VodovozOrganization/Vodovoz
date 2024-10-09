using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.MovementDocuments.InstanceAccounting
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения с автомобиля на склад(экземплярный учет)",
		Nominative = "строка перемещения с автомобиля на склад(экземплярный учет)")]
	public class InstanceMovementDocumentFromCarToWarehouseItem : InstanceMovementDocumentToWarehouseItem
	{
		public override MovementDocumentItemType MovementDocumentItemType =>
			MovementDocumentItemType.InstanceMovementDocumentFromCarToWarehouseItem;
		
		public virtual CarInstanceGoodsAccountingOperation WriteOffCarInstanceGoodsAccountingOperation
		{
			get => WriteOffOperation as CarInstanceGoodsAccountingOperation;
			set => WriteOffOperation = value;
		}
		
		public override bool TrySetIsUsed()
		{
			if(InventoryNomenclatureInstance != null && InventoryNomenclatureInstance.Nomenclature.HasConditionAccounting)
			{
				InventoryNomenclatureInstance.IsUsed = true;
				return true;
			}
			
			return base.TrySetIsUsed();
		}
		
		protected override void CreateOrUpdateWriteOffOperation()
		{
			if(WriteOffOperation is null)
			{
				WriteOffCarInstanceGoodsAccountingOperation = new CarInstanceGoodsAccountingOperation();
			}

			WriteOffCarInstanceGoodsAccountingOperation.InventoryNomenclatureInstance = InventoryNomenclatureInstance;
			WriteOffCarInstanceGoodsAccountingOperation.Car = Document.FromCar;
		}
	}
}
