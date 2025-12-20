using System;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Documents.DriverTerminalTransfer
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "документ переноса терминала другому водителю",
		NominativePlural = "документы переноса терминалов другому водителю")]
	[EntityPermission]
	[HistoryTrace]
	public class AnotherDriverTerminalTransferDocument : DriverTerminalTransferDocumentBase
	{
		private EmployeeNomenclatureMovementOperation _employeeNomenclatureMovementOperationFrom;
		private EmployeeNomenclatureMovementOperation _employeeNomenclatureMovementOperationTo;

		public virtual EmployeeNomenclatureMovementOperation EmployeeNomenclatureMovementOperationFrom
		{
			get => _employeeNomenclatureMovementOperationFrom;
			set => SetField(ref _employeeNomenclatureMovementOperationFrom, value);
		}

		public virtual EmployeeNomenclatureMovementOperation EmployeeNomenclatureMovementOperationTo
		{
			get => _employeeNomenclatureMovementOperationTo;
			set => SetField(ref _employeeNomenclatureMovementOperationTo, value);
		}

		public override string Title => $"Документ переноса терминала другому водителю №{ Id }";
	}
}
