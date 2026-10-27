using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Core.Domain.Interfaces.Orders
{
	/// <summary>
	/// Расчет доставки
	/// </summary>
	/// <typeparam name="T">Данные для расчета</typeparam>
	public interface IDeliveryPriceGetter<in T>
	{
		/// <summary>
		/// Получение цены доставки
		/// </summary>
		/// <param name="context">Данные для расчета доставки</param>
		/// <returns></returns>
		Result<decimal> GetDeliveryPrice(IDeliveryPriceGetterContext<T> context);
	}
}
