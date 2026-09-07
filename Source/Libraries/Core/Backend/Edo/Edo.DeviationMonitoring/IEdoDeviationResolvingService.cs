using System.Threading;
using System.Threading.Tasks;

namespace Edo.DeviationMonitoring
{
	/// <summary>
	/// Сервис снятия отклонений документооборота ЭДО.
	/// Работает только с задачами и заявками, по которым есть незакрытое отклонение:
	/// идет от самих отклонений, а не от окна просмотра, поэтому видит
	/// в том числе задачи, вышедшие за окно. Новых отклонений не заводит
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
