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
		private CalculatedOrder[] Nodes;
		PossibleTrip Trip;
		Dictionary<LogisticsArea, int> priorites;
		IDistanceCalculator distanceCalculator;

		public CallbackDistanceDistrict(CalculatedOrder[] nodes, PossibleTrip trip, IDistanceCalculator distanceCalculator)
		{
			Nodes = nodes;
			Trip = trip;
			priorites = trip.Districts.ToDictionary(x => x.District, x => x.Priority);
			this.distanceCalculator = distanceCalculator;
		}

		public override long Run(int first_index, int second_index)
		{
			if(first_index > Nodes.Length || second_index > Nodes.Length || first_index < 0 || second_index < 0)
			{
				logger.Error($"Get Distance {first_index} -> {second_index} out of orders ({Nodes.Length})");
				return 0;
			}

			if(first_index == second_index)
				return 0;

			if(second_index == 0)
				return distanceCalculator.DistanceToBaseMeter(Nodes[first_index - 1].Order.DeliveryPoint);

			bool fromExistRoute = false;
			if(Nodes[second_index - 1].ExistRoute != null && Trip.OldRoute != Nodes[second_index - 1].ExistRoute)
				return RouteOptimizer.RemoveOrderFromExistRLPenalty + GetSimpleDistance(first_index, second_index);
			else
				fromExistRoute = true;

			if(Trip.Car.TypeOfUse == CarTypeOfUse.Largus && Nodes[second_index - 1].Bootles > RouteOptimizer.MaxBottlesInOrderForLargus)
				return RouteOptimizer.LargusMaxBottlePenalty;	

			long distance;
			var aria = Nodes[second_index - 1].District;
			if(!priorites.ContainsKey(aria) && !fromExistRoute)
				return RouteOptimizer.UnlikeDistrictPenalty + GetSimpleDistance(first_index, second_index);

			if(first_index == 0)
				distance = distanceCalculator.DistanceFromBaseMeter(Nodes[second_index - 1].Order.DeliveryPoint);
			else
				distance = distanceCalculator.DistanceMeter(Nodes[first_index - 1].Order.DeliveryPoint, Nodes[second_index - 1].Order.DeliveryPoint);

			//Если адрес из уже существующего маршрута, не 
			return fromExistRoute ? distance : distance + priorites[aria] * RouteOptimizer.DistrictPriorityPenalty ;
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
