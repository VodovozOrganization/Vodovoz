using System.Threading.Tasks;
using DriverApi.Contracts.V6.Requests;
using Vodovoz.Core.Domain.Results;

namespace VodovozBusiness.NotificationSenders
{
	/// <summary>
	/// Отправка уведомлений об изменениях в МЛ
	/// </summary>
	public interface IRouteListChangesNotificationSender
	{
		/// <summary>
		/// Отправить уведомление об изменениях в МЛ
		/// </summary>
		/// <param name="changesRequest">Запрос на уведомление водителя об изменения в МЛ</param>
		/// <returns></returns>
		Task<Result> NotifyOfRouteListChanged(NotificationRouteListChangesRequest changesRequest);
	}
}
