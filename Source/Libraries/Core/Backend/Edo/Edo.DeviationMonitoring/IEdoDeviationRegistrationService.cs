using System.Threading;
using System.Threading.Tasks;

namespace Edo.DeviationMonitoring
{
	/// <summary>
	/// Сервис регистрации отклонений документооборота ЭДО
	/// </summary>
	public interface IEdoDeviationRegistrationService
	{
		/// <summary>
		/// Выполняет один проход регистрации отклонений
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат прохода</returns>
		Task<EdoDeviationRegistrationResult> RegisterAsync(CancellationToken cancellationToken);
	}
}
