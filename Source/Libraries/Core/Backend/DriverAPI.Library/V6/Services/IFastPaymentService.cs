using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Domain.FastPayments;

namespace DriverAPI.Library.V6.Services
{
	/// <summary>
	/// Интерфейс для работы с быстрыми платежами
	/// </summary>
	public interface IFastPaymentService
	{
		/// <summary>
		/// Получить статус быстрого платежа для заказа
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <param name="onlineOrder">Идентификатор онлайн-заказа (необязательно)</param>
		/// <returns>Статус быстрого платежа</returns>
		FastPaymentStatus? GetOrderFastPaymentStatus(int orderId, int? onlineOrder = null);
	}
}
