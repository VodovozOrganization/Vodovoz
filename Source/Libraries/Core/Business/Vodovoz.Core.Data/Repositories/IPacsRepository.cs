using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IPacsRepository
	{
		bool PacsEnabledFor(int employeeId);
		IEnumerable<OperatorState> GetOperatorStatesFrom(DateTime from);
		IEnumerable<OperatorState> GetOnlineOperators();
		IEnumerable<Call> GetCalls(DateTime from);
		IEnumerable<Call> GetActiveCalls();
		IEnumerable<string> GetAvailablePhones();
		DomainSettings GetPacsDomainSettings();

		IEnumerable<OperatorState> GetOperatorsOnBreak(DateTime date);
		IEnumerable<OperatorState> GetOperatorBreakStates(int operatorId, DateTime date);
		Task<Call> GetCallByEntryAsync(IUnitOfWork uow, string entryId);

		/// <summary>
		/// Получение истории событий звонка по идентификатору звонка
		/// </summary>
		/// <param name="callId">Идентификатор звонка</param>
		/// <returns></returns>
		Task<IEnumerable<CallEvent>> GetCallHistoryByCallIdAsync(string callId);
	}
}
