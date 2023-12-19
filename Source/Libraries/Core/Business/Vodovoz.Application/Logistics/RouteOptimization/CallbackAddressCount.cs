using Google.OrTools.ConstraintSolver;

namespace Vodovoz.Application.Services.Logistics.RouteOptimization
{
	/// <summary>
	/// Класс для всех адресов не являющихся складом погрузки и разгрузки, возвращает 1.
	/// Используется с измерением количества адресов.
	/// </summary>
	public class CallbackAddressCount : NodeEvaluator2
	{
		private int _ordersCount;

		public CallbackAddressCount(int ordersCount)
		{
			_ordersCount = ordersCount;
		}

		public override long Run(int first_index, int second_index)
		{
			return first_index != second_index && second_index != 0 && first_index <= _ordersCount && second_index <= _ordersCount ? 1 : 0;
		}
	}
}
