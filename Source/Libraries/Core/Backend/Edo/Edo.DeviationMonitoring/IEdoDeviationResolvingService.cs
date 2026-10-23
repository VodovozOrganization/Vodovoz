using System.Threading;
using System.Threading.Tasks;

namespace Edo.DeviationMonitoring
{
	/// <summary>
	/// Сервис проверки актуальности и обработки отклонений документооборота ЭДО
	/// </summary>
	public interface IEdoDeviationResolvingService
	{
		/// <summary>
		/// Снимает потерявшие актуальность отклонения и передает актуальные сервису автоматических действий
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат прохода</returns>
		Task<EdoDeviationResolvingResult> ResolveAsync(CancellationToken cancellationToken);
	}
}
