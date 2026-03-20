using CustomerOrdersApi.Library.Services.PaymentRefund;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Factories
{
	public interface IPaymentRefundServiceFactory
	{
		/// <summary>
		/// Получает сервис возврата по источнику оплаты
		/// </summary>
		IPaymentRefundService GetRefundService(OnlinePaymentSource? paymentSource);

		/// <summary>
		/// Получает сервис возврата по онлайн заказу
		/// </summary>
		IPaymentRefundService GetRefundService(OnlineOrder onlineOrder);
	}
}
