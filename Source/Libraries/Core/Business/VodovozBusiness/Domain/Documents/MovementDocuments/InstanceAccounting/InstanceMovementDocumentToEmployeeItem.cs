using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.MovementDocuments.InstanceAccounting
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения на сотрудника(экземплярный учет)",
		Nominative = "строка перемещения на сотрудника(экземплярный учет)")]
	public abstract class InstanceMovementDocumentToEmployeeItem : InstanceMovementDocumentItem
	{
		public virtual EmployeeInstanceGoodsAccountingOperation IncomeEmployeeInstanceGoodsAccountingOperation
		{
			get => IncomeOperation as EmployeeInstanceGoodsAccountingOperation;
			set => IncomeOperation = value;
		}
		
		protected override void CreateOrUpdateIncomeOperation()
		{
			if(IncomeOperation is null)
			{
				IncomeEmployeeInstanceGoodsAccountingOperation = new EmployeeInstanceGoodsAccountingOperation();
			}

			IncomeEmployeeInstanceGoodsAccountingOperation.InventoryNomenclatureInstance = InventoryNomenclatureInstance;
			IncomeEmployeeInstanceGoodsAccountingOperation.Employee = Document.ToEmployee;
		}
	}
}
