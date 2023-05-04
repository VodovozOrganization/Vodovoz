using System;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.WriteOffDocuments
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания с сотрудника(объемный учет)",
		Nominative = "строка списания с сотрудника(объемный учет)")]
	[HistoryTrace]
	public class BulkWriteOffFromEmployeeDocumentItem : BulkWriteOffDocumentItem
	{
		public override WriteOffDocumentItemType Type => WriteOffDocumentItemType.BulkWriteOffFromEmployeeDocumentItem;

		public virtual EmployeeBulkGoodsAccountingOperation EmployeeBulkGoodsAccountingOperation
		{
			get => GoodsAccountingOperation as EmployeeBulkGoodsAccountingOperation;
			set => GoodsAccountingOperation = value;
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

