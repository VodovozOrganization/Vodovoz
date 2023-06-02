using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.MovementDocuments.InstanceAccounting
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения с сотрудника на склад(экземплярный учет)",
		Nominative = "строка перемещения с сотрудника на склад(экземплярный учет)")]
	public class InstanceMovementDocumentFromEmployeeToWarehouseItem : InstanceMovementDocumentToWarehouseItem
	{
		public override MovementDocumentItemType MovementDocumentItemType =>
			MovementDocumentItemType.InstanceMovementDocumentFromEmployeeToWarehouseItem;
		
		public virtual EmployeeInstanceGoodsAccountingOperation WriteOffEmployeeInstanceGoodsAccountingOperation
		{
			get => WriteOffOperation as EmployeeInstanceGoodsAccountingOperation;
			set => WriteOffOperation = value;
		}
		
		protected override void CreateOrUpdateWriteOffOperation()
		{
			if(WriteOffOperation is null)
			{
				WriteOffEmployeeInstanceGoodsAccountingOperation = new EmployeeInstanceGoodsAccountingOperation();
			}

			WriteOffEmployeeInstanceGoodsAccountingOperation.InventoryNomenclatureInstance = InventoryNomenclatureInstance;
			WriteOffEmployeeInstanceGoodsAccountingOperation.Employee = Document.FromEmployee;
		}
	}
}
