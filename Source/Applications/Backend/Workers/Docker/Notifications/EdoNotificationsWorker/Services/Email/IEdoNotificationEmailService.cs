using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace EdoNotificationsWorker.Services.Email
{
	/// <summary>
	/// Рассылка уведомлений ЭДО по Email
	/// </summary>
	public interface IEdoNotificationEmailService
	{
		/// <summary>
		/// Отправить уведомления об ЭДО
		/// </summary>
		/// <param name="emails">Список email</param>
		/// <param name="subject">Тема письма</param>
		/// <param name="message">Текст уведомления</param>
		/// <param name="cancellationToken">Токен отмены</param>
		Task NotifyAsync(string emails, string subject, string message, CancellationToken cancellationToken = default);
	}
}
