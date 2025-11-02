using System.Collections.Generic;
using Vodovoz.Core.Domain;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.ViewModels.Services.RouteOptimization
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
		int TimeFromBaseSec(PointCoordinates fromBase, PointCoordinates toDeliveryPoint);
		int TimeSec(PointCoordinates fromDeliveryPont, PointCoordinates toDeliveryPoint);
		int TimeToBaseSec(PointCoordinates fromDeliveryPont, PointCoordinates toBase);
	}
}
