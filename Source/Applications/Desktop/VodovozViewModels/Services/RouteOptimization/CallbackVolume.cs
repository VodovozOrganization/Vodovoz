using Google.OrTools.ConstraintSolver;
using Microsoft.Extensions.Logging;

namespace Vodovoz.ViewModels.Services.RouteOptimization
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

		public override long Run(int firstIndex, int secondIndex)
		{
			if(firstIndex <= 0)
			{
				return 0;
			}

			if(firstIndex > _nodes.Length)
			{
				_logger.LogError("Get Weight {FirstIndex} -> {SecondIndex} out of orders ({NodesLength})", firstIndex, secondIndex, _nodes.Length);
				return 0;
			}

			return (long)(_nodes[firstIndex - 1].Volume * 1000);
		}
	}
}
