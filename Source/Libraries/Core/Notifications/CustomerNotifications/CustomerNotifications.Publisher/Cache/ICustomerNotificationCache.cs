using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace CustomerNotifications.Publisher.Cache
{
	/// <summary>
	/// Кэш для отметки отправленных уведомлений, предотвращающий повторную отправку.
	/// </summary>
	public interface ICustomerNotificationCache
	{
		/// <summary>
		/// Помечает уведомление как отправленное, если оно ещё не было отправлено.
		/// Возвращает true при первой отправке, иначе false.
		/// </summary>
		Task<bool> TryMarkAsFirstSentAsync(int onlineOrderId, CustomerNotificationEventType eventType);
	}
}
