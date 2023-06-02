using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.MovementDocuments.BulkAccounting
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения с сотрудника на сотрудника(объемный учет)",
		Nominative = "строка перемещения с сотрудника на сотрудника(объемный учет)")]
	public class BulkMovementDocumentFromEmployeeToEmployeeItem : BulkMovementDocumentToEmployeeItem
	{
		public override MovementDocumentItemType MovementDocumentItemType =>
			MovementDocumentItemType.BulkMovementDocumentFromEmployeeToEmployeeItem;
		
		public virtual EmployeeBulkGoodsAccountingOperation WriteOffEmployeeBulkGoodsAccountingOperation
		{
			get => WriteOffOperation as EmployeeBulkGoodsAccountingOperation;
			set => WriteOffOperation = value;
		}
		
		protected override void CreateOrUpdateWriteOffOperation()
		{
			if(WriteOffOperation is null)
			{
				WriteOffEmployeeBulkGoodsAccountingOperation = new EmployeeBulkGoodsAccountingOperation();
			}

			WriteOffEmployeeBulkGoodsAccountingOperation.Employee = Document.FromEmployee;
		}
	}
}
