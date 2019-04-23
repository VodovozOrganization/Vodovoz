using System;
using Vodovoz.Domain.Client;

namespace Vodovoz.Tools.Logistic
{
	public interface IDistanceCalculator
	{
		int DistanceMeter(DeliveryPoint fromDP, DeliveryPoint toDP);

		int DistanceFromBaseMeter(DeliveryPoint toDP);

		int DistanceToBaseMeter(DeliveryPoint fromDP);
	}
}
