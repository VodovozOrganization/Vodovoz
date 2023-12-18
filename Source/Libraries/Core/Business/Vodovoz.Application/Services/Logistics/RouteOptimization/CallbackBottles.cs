using System;
using Google.OrTools.ConstraintSolver;

namespace Vodovoz.Application.Services.Logistics.RouteOptimization
{
	/// <summary>
	/// Класс возвращает количесто бутылей в заказе на адресе.
	/// </summary>
	public class CallbackBottles : NodeEvaluator2
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private CalculatedOrder[] Nodes;

		public CallbackBottles(CalculatedOrder[] nodes)
		{
			Nodes = nodes;
		}

		public override long Run(int first_index, int second_index)
		{
			if(first_index <= 0)
			{
				return 0;
			}

			if(first_index > Nodes.Length)
			{
				logger.Error($"Get Bottles {first_index} -> {second_index} out of orders ({Nodes.Length})");
				return 0;
			}

			return Nodes[first_index - 1].Bottles;
		}
	}
}
