using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.MovementDocuments.BulkAccounting
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения на сотрудника(объемный учет)",
		Nominative = "строка перемещения на сотрудника(объемный учет)")]
	public abstract class BulkMovementDocumentToEmployeeItem : BulkMovementDocumentItem
	{
		public virtual EmployeeBulkGoodsAccountingOperation IncomeEmployeeBulkGoodsAccountingOperation
		{
			get => IncomeOperation as EmployeeBulkGoodsAccountingOperation;
			set => IncomeOperation = value;
		}
		
		protected override void CreateOrUpdateIncomeOperation()
		{
			if(IncomeOperation is null)
			{
				IncomeEmployeeBulkGoodsAccountingOperation = new EmployeeBulkGoodsAccountingOperation();
			}

			IncomeEmployeeBulkGoodsAccountingOperation.Employee = Document.ToEmployee;
		}
	}
}
