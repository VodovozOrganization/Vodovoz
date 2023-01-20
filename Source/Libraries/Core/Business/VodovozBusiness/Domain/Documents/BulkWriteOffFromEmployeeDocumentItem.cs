using System;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания с сотрудника(объемный учет)",
		Nominative = "строка списания с сотрудника(объемный учет)")]
	[HistoryTrace]
	public class BulkWriteOffFromEmployeeDocumentItem : BulkWriteOffDocumentItem
	{
		public virtual EmployeeBulkGoodsAccountingOperation EmployeeBulkGoodsAccountingOperation
		{
			get => WarehouseWriteOffOperation as EmployeeBulkGoodsAccountingOperation;
			set => WarehouseWriteOffOperation = value;
		}

		#region Функции

		public virtual void CreateOperation(Employee employee, DateTime time)
		{
			EmployeeBulkGoodsAccountingOperation = new EmployeeBulkGoodsAccountingOperation
			{
				Employee = employee,
				OperationTime = time,
			};
			
			FillOperation();
		}

		#endregion
	}
}

