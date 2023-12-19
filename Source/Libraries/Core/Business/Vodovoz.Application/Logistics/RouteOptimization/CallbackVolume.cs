using Google.OrTools.ConstraintSolver;
using Microsoft.Extensions.Logging;

namespace Vodovoz.Application.Logistics.RouteOptimization
{
	/// <summary>
	/// Класс обратного вызова, возвращает объем груза в заказе в кубических децеметрах
	/// </summary>
	public class CallbackVolume : NodeEvaluator2
	{
		private readonly ILogger<CallbackVolume> _logger;
		private CalculatedOrder[] _nodes;

		public CallbackVolume(ILogger<CallbackVolume> logger, CalculatedOrder[] nodes)
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

			return (long)(_nodes[first_index - 1].Volume * 1000);
		}
	}
}
