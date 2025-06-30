using Google.OrTools.ConstraintSolver;

namespace Vodovoz.ViewModels.Services.RouteOptimization
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

		public override long Run(int firstIndex, int secondIndex)
		{
			return firstIndex != secondIndex && secondIndex != 0 && firstIndex <= _ordersCount && secondIndex <= _ordersCount ? 1 : 0;
		}
	}
}
