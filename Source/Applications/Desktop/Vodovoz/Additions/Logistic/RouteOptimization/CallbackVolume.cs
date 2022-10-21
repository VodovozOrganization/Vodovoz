using System;
using Google.OrTools.ConstraintSolver;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	/// <summary>
	/// Класс обратного вызова, возвращает объем груза в заказе в кубических децеметрах
	/// </summary>
	public class CallbackVolume : NodeEvaluator2
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private CalculatedOrder[] Nodes;

		public CallbackVolume(CalculatedOrder[] nodes)
		{
			Nodes = nodes;
		}

		public override long Run(int first_index, int second_index)
		{
			if(first_index <= 0)
				return 0;

			if(first_index > Nodes.Length) {
				logger.Error($"Get Weight {first_index} -> {second_index} out of orders ({Nodes.Length})");
				return 0;
			}

			return (long)(Nodes[first_index - 1].Volume * 1000);
		}
	}
}
