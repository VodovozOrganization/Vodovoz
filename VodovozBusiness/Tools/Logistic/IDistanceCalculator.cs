using Vodovoz.Domain.Client;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.Tools.Logistic
{
	public interface IDistanceCalculator
	{
		int DistanceMeter(DeliveryPoint fromDP, DeliveryPoint toDP);

		int DistanceFromBaseMeter(GeographicGroup fromBase, DeliveryPoint toDP);

		int DistanceToBaseMeter(DeliveryPoint fromDP, GeographicGroup toBase);
	}
}
