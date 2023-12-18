using Google.OrTools.ConstraintSolver;

namespace Vodovoz.Application.Services.Logistics.RouteOptimization
{
	/// <summary>
	/// Класс обратного вызова, возвращает вес груза в заказе в килограммах
	/// </summary>
	public class CallbackWeight : NodeEvaluator2
	{
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private CalculatedOrder[] _nodes;

		public CallbackWeight(CalculatedOrder[] nodes)
		{
			_nodes = nodes;
		}

		public override long Run(int first_index, int second_index)
		{
			if(first_index <= 0)
			{
				return 0;
			}

			if(first_index > _nodes.Length)
			{
				_logger.Error($"Get Weight {first_index} -> {second_index} out of orders ({_nodes.Length})");
				return 0;
			}

			return (long)_nodes[first_index - 1].Weight;
		}
	}
}
