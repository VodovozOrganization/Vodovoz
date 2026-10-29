using System.Threading;
using System.Threading.Tasks;

namespace EdoNotificationsWorker.Services.Bitrix
{
	/// <summary>
	/// Рассылка уведомлений ЭДО в Битрикс
	/// </summary>
	public interface IEdoNotificationBitrixService
	{
		/// <summary>
		/// Отправить уведомления об ЭДО
		/// </summary>
		/// <param name="bitrixDialogs">Список email</param>
		/// <param name="message">Текст уведомления</param>
		/// <param name="cancellationToken">Токен отмены</param>
		Task NotifyAsync(string bitrixDialogs, string message, CancellationToken cancellationToken = default);
	}
}
