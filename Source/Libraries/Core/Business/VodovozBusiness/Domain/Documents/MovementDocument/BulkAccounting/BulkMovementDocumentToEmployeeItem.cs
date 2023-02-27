using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения на сотрудника(объемный учет)",
		Nominative = "строка перемещения на сотрудника(объемный учет)")]
	public abstract class BulkMovementDocumentToEmployeeItem : BulkMovementDocumentItem
	{
		public EmployeeBulkGoodsAccountingOperation IncomeEmployeeBulkGoodsAccountingOperation
		{
			get => IncomeOperation as EmployeeBulkGoodsAccountingOperation;
			set => IncomeOperation = value;
		}
		
		protected override void CreateIncomeOperation()
		{
			if(IncomeOperation is null)
			{
				IncomeEmployeeBulkGoodsAccountingOperation = new EmployeeBulkGoodsAccountingOperation();
			}
		}
		
		protected override void FillIncomeStorage()
		{
			IncomeEmployeeBulkGoodsAccountingOperation.Employee = Document.ToEmployee;
		}
	}
}
