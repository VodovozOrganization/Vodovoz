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
		AtWorkDriver Driver;
		Dictionary<LogisticsArea, int> priorites;
		IDistanceCalculator distanceCalculator;

		public CallbackDistanceDistrict(CalculatedOrder[] nodes, AtWorkDriver driver, IDistanceCalculator distanceCalculator)
		{
			Nodes = nodes;
			Driver = driver;
			priorites = driver.Employee.Districts.ToDictionary(x => x.District, x => x.Priority);
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

			long distance;
			var aria = Nodes[second_index - 1].District;
			if(!priorites.ContainsKey(aria))
			{
				if (first_index == 0)//РАССТОЯНИЯ ПРЯМЫЕ без спутника.
					return RouteOptimizer.UnlikeDistrictPenalty + (int)(DistanceCalculator.GetDistanceFromBase(Nodes[second_index - 1].Order.DeliveryPoint) * 1000);
				else
					return RouteOptimizer.UnlikeDistrictPenalty + (int)(DistanceCalculator.GetDistance(Nodes[first_index - 1].Order.DeliveryPoint, Nodes[second_index - 1].Order.DeliveryPoint) * 1000);
			}


			if(first_index == 0)
				distance = distanceCalculator.DistanceFromBaseMeter(Nodes[second_index - 1].Order.DeliveryPoint);
			else
				distance = distanceCalculator.DistanceMeter(Nodes[first_index - 1].Order.DeliveryPoint, Nodes[second_index - 1].Order.DeliveryPoint);

			return distance + priorites[aria] * RouteOptimizer.DistrictPriorityPenalty ;
		}
	}
}
