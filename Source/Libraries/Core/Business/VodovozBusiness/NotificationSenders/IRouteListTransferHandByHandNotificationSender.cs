using System.Threading.Tasks;
using Vodovoz.Errors;

namespace Vodovoz.NotificationSenders
{
	/// <summary>
	/// Отправка уведомлений о передаче товара из рук в руки
	/// </summary>
	public interface IRouteListTransferHandByHandNotificationSender
	{
		/// <summary>
		/// Отправить уведомление о передаче товара из рук в руки
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <returns></returns>
		Task<Result> NotifyOfOrderWithGoodsTransferingIsTransfered(int orderId);
	}
}
