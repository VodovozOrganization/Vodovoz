using System.Threading;
using System.Threading.Tasks;

namespace Edo.Problem.Routine.Services.ReceiptContactProblem
{
	/// <summary>
	/// Сервис обработки активных проблем с контактом для отправки чека.
	/// </summary>
	public interface IReceiptContactProblemService
	{
		/// <summary>
		/// Обрабатывает задачи ЭДО с активной проблемой контакта для отправки чека.
		/// </summary>
		/// <param name="cancellationToken">Токен отмены.</param>
		Task ProcessContactProblems(CancellationToken cancellationToken);
	}
}
