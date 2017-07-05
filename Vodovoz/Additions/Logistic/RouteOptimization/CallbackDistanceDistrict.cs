using System.Collections.Generic;
using Google.OrTools.ConstraintSolver;
using Vodovoz.Domain.Logistic;
using System.Linq;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class CallbackDistanceDistrict : NodeEvaluator2
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private CalculatedOrder[] Nodes;
		AtWorkDriver Driver;

		public CallbackDistanceDistrict(CalculatedOrder[] nodes, AtWorkDriver driver)
		{
			Nodes = nodes;
			Driver = driver;
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
				return (long)(DistanceCalculator.GetDistanceToBase(Nodes[first_index - 1].Order.DeliveryPoint) * 1000);

			long distance;
			var aria = Nodes[second_index - 1].District;
			var priority = Driver.Employee.Districts.FirstOrDefault(x => x.District == aria);
			if(priority == null)
				return 100000;

			if(first_index == 0)
				distance = (long)(DistanceCalculator.GetDistanceFromBase(Nodes[second_index - 1].Order.DeliveryPoint) * 1000);
			else
				distance = (long)(DistanceCalculator.GetDistance(Nodes[first_index - 1].Order.DeliveryPoint, Nodes[second_index - 1].Order.DeliveryPoint) * 1000);

			return distance + priority.Priority * 1000 ; // приоритет = 1 км. Можно умножить на нужное количество км.
		}
	}
}
