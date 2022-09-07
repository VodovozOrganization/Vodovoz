using Vodovoz.Domain.Client;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Tools.Logistic
{
	public interface IDistanceCalculator
	{
		int DistanceMeter(DeliveryPoint fromDP, DeliveryPoint toDP);

		int DistanceFromBaseMeter(GeoGroupVersion fromBase, DeliveryPoint toDP);

		int DistanceToBaseMeter(DeliveryPoint fromDP, GeoGroupVersion toBase);
	}
}
