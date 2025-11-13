using System.Threading.Tasks;

namespace Vodovoz.NotificationSenders
{
	/// <summary>
	/// Отправка уведомлений, что заявка на выдачу наличных денежных средств переведена в статус "Передана на выдачу"
	/// </summary>
	public interface ICashRequestForDriverIsGivenForTakeNotificationSender
	{
		/// <summary>
		/// Отправить уведомление, что заявка на выдачу наличных денежных средств переведена в статус "Передана на выдачу"
		/// </summary>
		/// <param name="cashRequestId">Номер заявки на выдачу наличных ДС</param>
		/// <returns></returns>
		Task NotifyOfCashRequestForDriverIsGivenForTake(int cashRequestId);
	}
}
