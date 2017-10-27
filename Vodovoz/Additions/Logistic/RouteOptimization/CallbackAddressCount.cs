using System;
using Google.OrTools.ConstraintSolver;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	/// <summary>
	/// Класс для всех адресов не являющихся складом погрузки и разгрузки, возвращает 1.
	/// Используется с измерением количества адресов.
	/// </summary>
	public class CallbackAddressCount : NodeEvaluator2
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		int ordersCount;

		public CallbackAddressCount(int ordersCount)
		{
			this.ordersCount = ordersCount;
		}

		public override long Run(int first_index, int second_index)
		{
			return (first_index != second_index && second_index != 0 && first_index <= ordersCount && second_index <= ordersCount) ? 1 : 0;
		}
	}
}
