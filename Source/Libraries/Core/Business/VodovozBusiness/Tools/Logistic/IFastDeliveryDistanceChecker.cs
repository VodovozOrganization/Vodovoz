using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Tools.Logistic
{
	public interface IFastDeliveryDistanceChecker
	{
		bool DeliveryPointInFastDeliveryRadius(DeliveryPoint deliveryPoint, DriverPositionWithFastDeliveryRadius driverPosition);
	}
}