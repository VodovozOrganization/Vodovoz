using Vodovoz.Core.Domain;

namespace Vodovoz.Tools.Logistic
{
	public interface IDistanceCalculator
	{
		int DistanceMeter(PointCoordinates fromDeliveryPoint, PointCoordinates toDeliveryPoint);

		int DistanceFromBaseMeter(PointCoordinates fromBase, PointCoordinates toDeliveryPoint);

		int DistanceToBaseMeter(PointCoordinates fromDeliveryPoint, PointCoordinates toBase);
	}
}
