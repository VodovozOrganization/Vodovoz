using Vodovoz.Domain.Logistic;

namespace Vodovoz.Application.Services.Logistics
{
	public interface IProposedRoute
	{
		void UpdateAddressOrderInRealRoute(RouteList updatedRoute);
	}
}
