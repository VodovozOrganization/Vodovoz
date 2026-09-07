using System.Threading;
using System.Threading.Tasks;

namespace Edo.DeviationMonitoring
{
	/// <summary>
	/// Сервис регистрации отклонений документооборота ЭДО.
	/// Идет от окна просмотра: обходит незавершенные задачи и заявки без задач,
	/// прогоняет их через валидаторы и заводит новые отклонения.
	/// Отклонения не закрывает — этим занимается <see cref="IEdoDeviationResolvingService"/>
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
