using Vodovoz.Domain.Client;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Tools.Logistic
{
	public interface IDistanceCalculator
	{
		int DistanceMeter(DeliveryPoint fromDP, DeliveryPoint toDP);

		int DistanceFromBaseMeter(GeoGroup fromBase, DeliveryPoint toDP);

		int DistanceToBaseMeter(DeliveryPoint fromDP, GeoGroup toBase);
	}
}
