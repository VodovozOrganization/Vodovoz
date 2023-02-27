using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения с сотрудника на склад(объемный учет)",
		Nominative = "строка перемещения с сотрудника на склад(объемный учет)")]
	public class BulkMovementDocumentFromEmployeeToWarehouseItem : BulkMovementDocumentToWarehouseItem
	{
		public EmployeeBulkGoodsAccountingOperation WriteOffEmployeeBulkGoodsAccountingOperation
		{
			get => WriteOffOperation as EmployeeBulkGoodsAccountingOperation;
			set => WriteOffOperation = value;
		}
		
		protected override void CreateWriteOffOperation()
		{
			if(WriteOffOperation is null)
			{
				WriteOffEmployeeBulkGoodsAccountingOperation = new EmployeeBulkGoodsAccountingOperation();
			}
		}
		
		protected override void FillWriteOffStorage()
		{
			WriteOffEmployeeBulkGoodsAccountingOperation.Employee = Document.FromEmployee;
		}
	}
}
