using Vodovoz.Domain.Payments;

namespace VodovozBusiness.Domain.Payments
{
	/// <summary>
	/// Фабрика создания билдеров онлайн платежей
	/// </summary>
	public interface IPaymentByCardOnlineBuilderFactory
	{
		/// <summary>
		/// Создание нужного билдера для онлайн платежа из выписки
		/// </summary>
		/// <param name="registerData"></param>
		/// <returns></returns>
		IPaymentByCardOnlineBuilder Create((IOnlinePaymentRegisterColumns Columns, PaymentByCardOnlineFrom? PaymentFrom) registerData);
	}
}
