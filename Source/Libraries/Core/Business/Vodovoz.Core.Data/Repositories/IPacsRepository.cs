using System.Collections.Generic;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IPacsRepository
	{
		bool PacsEnabledFor(int subdivisionId);
		IEnumerable<OperatorState> GetOnlineOperators();
		IEnumerable<CallEvent> GetActiveCalls();
		IEnumerable<string> GetAvailablePhones();
		DomainSettings GetPacsDomainSettings();
	}
}
