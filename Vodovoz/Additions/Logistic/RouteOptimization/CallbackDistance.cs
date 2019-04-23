using Google.OrTools.ConstraintSolver;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{

	/// <summary>
	/// Класс обратного вызова, для рассчета расстояние без штрафов используется в построении одного маршрута.
	/// </summary>
	public class CallbackDistance : NodeEvaluator2
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private CalculatedOrder[] Nodes;
		IDistanceCalculator distanceCalculator;

		public CallbackDistance(CalculatedOrder[] nodes, IDistanceCalculator distanceCalculator)
		{
			Nodes = nodes;
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

			long distance;

			if(first_index == 0)
				distance = distanceCalculator.DistanceFromBaseMeter(Nodes[second_index - 1].Order.DeliveryPoint);
			else if(second_index == 0)
				distance = distanceCalculator.DistanceToBaseMeter(Nodes[first_index - 1].Order.DeliveryPoint);
			else
				distance = distanceCalculator.DistanceMeter(Nodes[first_index - 1].Order.DeliveryPoint, Nodes[second_index - 1].Order.DeliveryPoint);

			return distance;
		}
	}
}
