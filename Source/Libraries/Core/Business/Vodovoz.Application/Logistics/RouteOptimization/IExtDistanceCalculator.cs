using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Application.Logistics.RouteOptimization
{
	public interface IExtDistanceCalculator : IDistanceCalculator
	{
		bool Canceled { get; set; }
#if DEBUG
		int[,] MatrixCount { get; }
#endif
		List<WayHash> ErrorWays { get; }
		void Dispose();
		void FlushCache();
		int TimeFromBaseSec(GeoGroupVersion fromBase, DeliveryPoint toDP);
		int TimeSec(DeliveryPoint fromDP, DeliveryPoint toDP);
		int TimeToBaseSec(DeliveryPoint fromDP, GeoGroupVersion toBase);
	}
}
