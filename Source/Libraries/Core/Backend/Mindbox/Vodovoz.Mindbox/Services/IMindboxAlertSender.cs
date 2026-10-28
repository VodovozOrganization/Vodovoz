using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Mindbox.Services
{
	/// <summary>
	/// Отправляет алерты по ошибкам интеграции
	/// </summary>
	public interface IMindboxAlertSender
	{
		Task AlertAsync(string message, CancellationToken cancellationToken);
	}
}
