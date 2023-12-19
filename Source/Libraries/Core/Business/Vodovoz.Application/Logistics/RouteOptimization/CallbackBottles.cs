using Google.OrTools.ConstraintSolver;
using Microsoft.Extensions.Logging;

namespace Vodovoz.Application.Services.Logistics.RouteOptimization
{
	/// <summary>
	/// Класс возвращает количесто бутылей в заказе на адресе.
	/// </summary>
	public class CallbackBottles : NodeEvaluator2
	{
		private readonly ILogger<CallbackBottles> _logger;
		private CalculatedOrder[] _nodes;

		public CallbackBottles(ILogger<CallbackBottles> logger, CalculatedOrder[] nodes)
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
				_logger.LogError($"Get Bottles {first_index} -> {second_index} out of orders ({_nodes.Length})");
				return 0;
			}

			return _nodes[first_index - 1].Bottles;
		}
	}
}
