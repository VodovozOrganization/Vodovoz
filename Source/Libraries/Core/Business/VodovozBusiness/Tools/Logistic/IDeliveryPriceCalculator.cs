using Vodovoz.Domain.Client;

namespace Vodovoz.Tools.Logistic
{
	public interface IDeliveryPriceCalculator
	{
		DeliveryPriceNode Calculate(decimal? latitude, decimal? longitude);
		DeliveryPriceNode Calculate(decimal? latitude, decimal? longitude, int? bottlesCount);
		DeliveryPriceNode Calculate(DeliveryPoint point, int? bottlesCount = null);
		DeliveryPriceNode CalculateForService(DeliveryPoint point);
	}
}