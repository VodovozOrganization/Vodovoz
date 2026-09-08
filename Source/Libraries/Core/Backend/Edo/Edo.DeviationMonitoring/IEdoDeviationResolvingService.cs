using System.Threading;
using System.Threading.Tasks;

namespace Edo.DeviationMonitoring
{
	/// <summary>
	/// Сервис снятия отклонений документооборота ЭДО
	/// </summary>
	public interface IEdoDeviationResolvingService
	{
		/// <summary>
		/// Выполняет один проход снятия потерявших актуальность отклонений
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат прохода</returns>
		Task<EdoDeviationResolvingResult> ResolveAsync(CancellationToken cancellationToken);
	}
}
