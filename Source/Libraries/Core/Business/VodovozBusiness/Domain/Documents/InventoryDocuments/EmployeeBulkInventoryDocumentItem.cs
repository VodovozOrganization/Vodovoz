using System;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.InventoryDocuments
{
	public class EmployeeBulkInventoryDocumentItem : InventoryDocumentItem
	{
		public override InventoryDocumentType Type => InventoryDocumentType.EmployeeInventory;

		protected override void CreateOperation(DateTime time)
		{
			GoodsAccountingOperation = new EmployeeBulkGoodsAccountingOperation
			{
				Employee = Document.Employee
			};
			FillOperation(time);
		}
		
		protected override void UpdateOperation()
		{
			EmployeeBulkChangeOperation.Employee = Document.Employee;
			base.UpdateOperation();
		}

		public virtual EmployeeBulkGoodsAccountingOperation EmployeeBulkChangeOperation
		{
			get => GoodsAccountingOperation as EmployeeBulkGoodsAccountingOperation;
			set => GoodsAccountingOperation = value;
		}
	}
}
