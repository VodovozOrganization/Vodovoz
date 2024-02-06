using Pacs.Server;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IPacsRepository
	{
		bool PacsEnabledFor(int employeeId);
		IEnumerable<OperatorState> GetOperators(DateTime from);
		IEnumerable<OperatorState> GetOnlineOperators();
		IEnumerable<CallEvent> GetCalls(DateTime from);
		IEnumerable<CallEvent> GetActiveCalls();
		IEnumerable<string> GetAvailablePhones();
		DomainSettings GetPacsDomainSettings();

		IEnumerable<OperatorState> GetOperatorsOnBreak(DateTime date);
		IEnumerable<OperatorState> GetOperatorBreakStates(int operatorId, DateTime date);
		Operator GetOperator(int operatorId);
	}
}
