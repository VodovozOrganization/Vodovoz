using System;
using Google.OrTools.ConstraintSolver;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class CallbackDistance : NodeEvaluator2
	{
		private CalculatedOrder[] Nodes;

		public CallbackDistance(CalculatedOrder[] nodes)
		{
			Nodes = nodes;
		}

		public override long Run(int first_index, int second_index)
		{
			if(first_index == 0)
				return (long)(DistanceCalculator.GetDistanceFromBase(Nodes[second_index - 1].Order.DeliveryPoint) * 1000);
			if(second_index == 0)
				return (long)(DistanceCalculator.GetDistanceToBase(Nodes[first_index - 1].Order.DeliveryPoint) * 1000);
			return (long)(DistanceCalculator.GetDistance(Nodes[first_index - 1].Order.DeliveryPoint, Nodes[second_index - 1].Order.DeliveryPoint) * 1000);
		}
	}
}
