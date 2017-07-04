using System;
using Google.OrTools.ConstraintSolver;
using QSProjectsLib;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class CallbackDistance : NodeEvaluator2
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private CalculatedOrder[] Nodes;
		//long[,] cachedDistance;

		public CallbackDistance(CalculatedOrder[] nodes)
		{
			Nodes = nodes;
			//cachedDistance = new long[nodes.Length + 1, nodes.Length + 1];
			//for(int x = 0; x < Nodes.Length; x++)
			//	for(int y = 0; y < Nodes.Length; y++)
			//{
			//	cachedDistance[y, x] = Run(y, x);
			//}
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

			long distance;

			if(first_index == 0)
				distance = (long)(DistanceCalculator.GetDistanceFromBase(Nodes[second_index - 1].Order.DeliveryPoint) * 1000);
			else if(second_index == 0)
				distance = (long)(DistanceCalculator.GetDistanceToBase(Nodes[first_index - 1].Order.DeliveryPoint) * 1000);
			else
				distance = (long)(DistanceCalculator.GetDistance(Nodes[first_index - 1].Order.DeliveryPoint, Nodes[second_index - 1].Order.DeliveryPoint) * 1000);

			return distance;
		}
	}
}
