using System;

namespace Vodovoz.Models
{
	public interface IRouteListDailyNumberProvider
	{
		int GetOrCreateDailyNumber(int routeListId, DateTime date);
	}
}
