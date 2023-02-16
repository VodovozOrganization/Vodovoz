using System.Collections.Generic;

namespace RevenueService.Client
{
	interface IRevenueServiceClient
	{
		IList<RevenueServiceCounterpartyDto> GetCounterpartyInfoFromRevenueService(string inn, string kpp = null);
	}
}
