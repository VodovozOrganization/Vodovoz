using System;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.DriverTerminalTransfer
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "документ переноса терминала для одного водителя",
		NominativePlural = "документы переноса терминалов для одного водителя")]
	[EntityPermission]
	[HistoryTrace]
	public class SelfDriverTerminalTransferDocument : DriverTerminalTransferDocumentBase
	{
		public override string Title => $"Документ переноса терминала для одного водителя №{ Id }";
	}
}
