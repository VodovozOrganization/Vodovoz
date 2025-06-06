using System.Threading.Tasks;
using DriverApi.Contracts.V6.Requests;
using Vodovoz.Core.Domain.Results;

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
		/// <param name="changesRequest">Запрос на уведомление водителя об изменения в МЛ</param>
		/// <returns></returns>
		Task<Result> NotifyOfOrderWithGoodsTransferingIsTransfered(NotificationRouteListChangesRequest changesRequest);
	}
}
