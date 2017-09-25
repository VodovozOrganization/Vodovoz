using System.Collections.Generic;
using System.Linq;
using Google.OrTools.ConstraintSolver;
using Vodovoz.Domain.Logistic;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class CallbackDistanceDistrict : NodeEvaluator2
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
#if DEBUG
		public static Dictionary<PossibleTrip, int> SGoToBase = new Dictionary<PossibleTrip, int>();
		public static Dictionary<PossibleTrip, int> SFromExistPenality = new Dictionary<PossibleTrip, int>();
		public static Dictionary<PossibleTrip, int> SUnlikeDistrictPenality = new Dictionary<PossibleTrip, int>();
		public static Dictionary<PossibleTrip, int> SLargusPenality = new Dictionary<PossibleTrip, int>();
#endif

		private CalculatedOrder[] Nodes;
		PossibleTrip Trip;
		Dictionary<LogisticsArea, int> priorites;
		IDistanceCalculator distanceCalculator;
		long fixedAddressPenality;

		public CallbackDistanceDistrict(CalculatedOrder[] nodes, PossibleTrip trip, IDistanceCalculator distanceCalculator)
		{
			Nodes = nodes;
			Trip = trip;
			priorites = trip.Districts.ToDictionary(x => x.District, x => x.Priority);
			fixedAddressPenality = RouteOptimizer.DriverPriorityAddressPenalty * (Trip.DriverPriority - 1);
			this.distanceCalculator = distanceCalculator;
#if DEBUG
			SGoToBase[Trip] = 0;
			SFromExistPenality[Trip] = 0;
			SUnlikeDistrictPenality[Trip] = 0;
			SLargusPenality[Trip] = 0;
#endif
		}

		public override long Run(int first_index, int second_index)
		{
			if(first_index > Nodes.Length || second_index > Nodes.Length || first_index < 0 || second_index < 0) {
				logger.Error($"Get Distance {first_index} -> {second_index} out of orders ({Nodes.Length})");
				return 0;
			}

			if(first_index == second_index)
				return 0;

			if(second_index == 0) {
#if DEBUG
				SGoToBase[Trip]++;
#endif
				return distanceCalculator.DistanceToBaseMeter(Nodes[first_index - 1].Order.DeliveryPoint);
			}

			bool fromExistRoute = false;
			if(Nodes[second_index - 1].ExistRoute != null) {
				if(Trip.OldRoute != Nodes[second_index - 1].ExistRoute) {
#if DEBUG
					SFromExistPenality[Trip]++;
#endif
					return RouteOptimizer.RemoveOrderFromExistRLPenalty + GetSimpleDistance(first_index, second_index);
				}
				fromExistRoute = true;
			}

			bool addressForLargus = Nodes[second_index - 1].Bootles <= RouteOptimizer.MaxBottlesInOrderForLargus;
			long distance = 0;

			if(Trip.Car.TypeOfUse == CarTypeOfUse.Largus && !addressForLargus) {
#if DEBUG
				SLargusPenality[Trip]++;
#endif
				return RouteOptimizer.LargusMaxBottlePenalty;
			}

			if(Trip.Car.TypeOfUse != CarTypeOfUse.Largus && addressForLargus)
				distance += RouteOptimizer.SmallOrderNotLargusPenalty;

			var aria = Nodes[second_index - 1].District;

			if(!fromExistRoute)//Если адрес из уже существующего маршрута, не учитываем приоритеты районов
			{
				if(priorites.ContainsKey(aria))
					distance += priorites[aria] * RouteOptimizer.DistrictPriorityPenalty;
				else{
#if DEBUG
					SUnlikeDistrictPenality[Trip]++;
#endif
					distance += RouteOptimizer.UnlikeDistrictPenalty;
				}
			}

			if(first_index == 0)
				distance += distanceCalculator.DistanceFromBaseMeter(Nodes[second_index - 1].Order.DeliveryPoint);
			else
				distance += distanceCalculator.DistanceMeter(Nodes[first_index - 1].Order.DeliveryPoint, Nodes[second_index - 1].Order.DeliveryPoint);

			return distance + fixedAddressPenality;
		}

		private long GetSimpleDistance(int first_index, int second_index)
		{
			if(first_index == 0)//РАССТОЯНИЯ ПРЯМЫЕ без спутника.
				return (long)(DistanceCalculator.GetDistanceFromBase(Nodes[second_index - 1].Order.DeliveryPoint) * 1000);
			else
				return (long)(DistanceCalculator.GetDistance(Nodes[first_index - 1].Order.DeliveryPoint, Nodes[second_index - 1].Order.DeliveryPoint) * 1000);
		}
	}
}
