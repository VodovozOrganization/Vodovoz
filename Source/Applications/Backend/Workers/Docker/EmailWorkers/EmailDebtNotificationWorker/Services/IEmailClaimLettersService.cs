using System.Threading;
using System.Threading.Tasks;

namespace EmailDebtNotificationWorker.Services
{
	/// <summary>
	/// Сервис для отправки претензионных писем должникам, у которых просрочка по платежу больше, чем заданное количество дней сверх ПДЗ
	/// </summary>
	public interface IEmailClaimLettersService
	{
		/// <summary>
		/// Отправляет претензионные письма должникам, у которых просрочка по платежу больше, чем заданное количество дней сверх ПДЗ. 
		/// Количество отправляемых писем за один цикл ограничено настройками. 
		/// Также учитывается количество уже отправленных писем за день, чтобы не превышать дневной лимит
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task SendClaimLetters(CancellationToken cancellationToken);
	}
}
