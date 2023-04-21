﻿using System;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания с сотрудника(экземплярный учет)",
		Nominative = "строка списания с сотрудника(экземплярный учет)")]
	[HistoryTrace]
	public class InstanceWriteOffFromEmployeeDocumentItem : InstanceWriteOffDocumentItem
	{
		public virtual EmployeeInstanceGoodsAccountingOperation EmployeeInstanceGoodsAccountingOperation
		{
			get => GoodsAccountingOperation as EmployeeInstanceGoodsAccountingOperation;
			set => GoodsAccountingOperation = value;
		}

		#region Функции

		public virtual void CreateOperation(Employee employee, DateTime time)
		{
			EmployeeInstanceGoodsAccountingOperation = new EmployeeInstanceGoodsAccountingOperation
			{
				Employee = employee,
				OperationTime = time,
			};
			
			FillOperation();
		}

		#endregion
	}
}

