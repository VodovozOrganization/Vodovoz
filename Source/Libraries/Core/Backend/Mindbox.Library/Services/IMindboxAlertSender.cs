using System.Threading;
using System.Threading.Tasks;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Отправляет алерты по ошибкам интеграции
	/// </summary>
	public interface IMindboxAlertSender
	{
		/// <summary>
		/// Отправляет алерт об ошибке интеграции
		/// </summary>
		/// <param name="message">Текст алерта</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Задача отправки алерта</returns>
		Task AlertAsync(string message, CancellationToken cancellationToken);
	}
}
