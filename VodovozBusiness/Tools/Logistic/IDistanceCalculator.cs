using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.Tools.Logistic
{
	public interface IDistanceCalculator
	{
		int DistanceMeter(DeliveryPoint fromDP, DeliveryPoint toDP, DateTime? activationTimeOneVersion, DateTime? activationTimeTwoVersion);

		int DistanceFromBaseMeter(GeographicGroup fromBase, DeliveryPoint toDP, DateTime? activationTime);

		int DistanceToBaseMeter(DeliveryPoint fromDP, GeographicGroup toBase, DateTime? activationTime);
	}
}
