using CustomerOrdersApi.Library.Services.PaymentRefund;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Default.Factories
{
	public interface IPaymentRefundServiceFactory
	{
		/// <summary>
		/// Получает сервис возврата по источнику оплаты
		/// </summary>
		/// <param name="paymentSource">Источник оплаты</param>
		/// <returns>Сервис по возврату платежа</returns>
		IPaymentRefundService GetRefundService(OnlinePaymentSource? paymentSource);

		/// <summary>
		/// Получает сервис возврата по онлайн заказу
		/// </summary>
		/// <param name="onlineOrder">Онлайн заказ</param>
		/// <returns>Сервис по возврату платежа</returns>
		IPaymentRefundService GetRefundService(OnlineOrder onlineOrder);
	}
}
