using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения на сотрудника(экземплярный учет)",
		Nominative = "строка перемещения на сотрудника(экземплярный учет)")]
	public abstract class InstanceMovementDocumentToEmployeeItem : InstanceMovementDocumentItem
	{
		public EmployeeInstanceGoodsAccountingOperation IncomeEmployeeInstanceGoodsAccountingOperation
		{
			get => IncomeOperation as EmployeeInstanceGoodsAccountingOperation;
			set => IncomeOperation = value;
		}
		
		protected override void CreateIncomeOperation()
		{
			if(IncomeOperation is null)
			{
				IncomeEmployeeInstanceGoodsAccountingOperation = new EmployeeInstanceGoodsAccountingOperation();
			}
		}
		
		protected override void FillIncomeStorage()
		{
			IncomeEmployeeInstanceGoodsAccountingOperation.Employee = Document.ToEmployee;
		}
	}
}
