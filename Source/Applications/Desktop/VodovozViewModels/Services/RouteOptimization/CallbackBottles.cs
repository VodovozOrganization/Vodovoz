using Google.OrTools.ConstraintSolver;
using Microsoft.Extensions.Logging;

namespace Vodovoz.ViewModels.Services.RouteOptimization
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

		public override long Run(int firstIndex, int secondIndex)
		{
			if(firstIndex <= 0)
			{
				return 0;
			}

			if(firstIndex > _nodes.Length)
			{
				_logger.LogError("Get Bottles {FirstIndex} -> {SecondIndex} out of orders ({NodesLength})", firstIndex, secondIndex, _nodes.Length);
				return 0;
			}

			return _nodes[firstIndex - 1].Bottles;
		}
	}
}
