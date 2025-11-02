using Vodovoz.Domain;

namespace DriverAPI.Library.V6.Services
{
	/// <summary>
	/// Интерфейс для работы с оплатой через СМС
	/// </summary>
	public interface ISmsPaymentService
	{
		/// <summary>
		/// Получить статус оплаты заказа через СМС
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>Статус оплаты через СМС</returns>
		SmsPaymentStatus? GetOrderSmsPaymentStatus(int orderId);
	}
}
