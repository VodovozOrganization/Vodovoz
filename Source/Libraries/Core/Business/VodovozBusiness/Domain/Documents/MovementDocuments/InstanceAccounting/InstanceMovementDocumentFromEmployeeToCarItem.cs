using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.MovementDocuments.InstanceAccounting
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения с сотрудника на автомобиль(экземплярный учет)",
		Nominative = "строка перемещения с сотрудника на автомобиль(экземплярный учет)")]
	public class InstanceMovementDocumentFromEmployeeToCarItem : InstanceMovementDocumentToCarItem
	{
		public override MovementDocumentItemType MovementDocumentItemType =>
			MovementDocumentItemType.InstanceMovementDocumentFromEmployeeToCarItem;
		
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
