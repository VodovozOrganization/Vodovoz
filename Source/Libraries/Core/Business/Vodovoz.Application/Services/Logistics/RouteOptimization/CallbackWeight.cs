using Google.OrTools.ConstraintSolver;
using Microsoft.Extensions.Logging;

namespace Vodovoz.Application.Services.Logistics.RouteOptimization
{
	/// <summary>
	/// Класс обратного вызова, возвращает вес груза в заказе в килограммах
	/// </summary>
	public class CallbackWeight : NodeEvaluator2
	{
		private static ILogger<CallbackWeight> _logger;
		private CalculatedOrder[] _nodes;

		public CallbackWeight(ILogger<CallbackWeight> logger, CalculatedOrder[] nodes)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
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
				_logger.LogError($"Get Weight {first_index} -> {second_index} out of orders ({_nodes.Length})");
				return 0;
			}

			return (long)_nodes[first_index - 1].Weight;
		}
	}
}
