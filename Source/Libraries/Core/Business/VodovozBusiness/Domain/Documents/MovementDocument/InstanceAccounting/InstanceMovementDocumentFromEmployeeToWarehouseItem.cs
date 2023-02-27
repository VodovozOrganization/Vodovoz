using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения с сотрудника на склад(экземплярный учет)",
		Nominative = "строка перемещения с сотрудника на склад(экземплярный учет)")]
	public class InstanceMovementDocumentFromEmployeeToWarehouseItem : InstanceMovementDocumentToWarehouseItem
	{
		public EmployeeInstanceGoodsAccountingOperation WriteOffEmployeeInstanceGoodsAccountingOperation
		{
			get => WriteOffOperation as EmployeeInstanceGoodsAccountingOperation;
			set => WriteOffOperation = value;
		}
		
		protected override void CreateWriteOffOperation()
		{
			if(WriteOffOperation is null)
			{
				WriteOffEmployeeInstanceGoodsAccountingOperation = new EmployeeInstanceGoodsAccountingOperation();
			}
		}
		
		protected override void FillWriteOffStorage()
		{
			WriteOffEmployeeInstanceGoodsAccountingOperation.Employee = Document.FromEmployee;
		}
	}
}
