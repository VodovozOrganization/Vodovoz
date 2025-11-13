using Vodovoz.Domain.Payments;

namespace VodovozBusiness.Domain.Payments
{
	/// <summary>
	/// Билдер онлайн платежей
	/// </summary>
	public interface IPaymentByCardOnlineBuilder
	{
		/// <summary>
		/// Создание онлайн платежа из выписки
		/// </summary>
		/// <param name="data">Строка с данными выписки</param>
		/// <returns>Онлайн платеж <see cref="PaymentByCardOnline"/></returns>
		PaymentByCardOnline Build(string[] data);
	}
}
