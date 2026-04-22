using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Core.Application.Problems.Services
{
	public interface IOrderContactProblemUpdateService
	{
		/// <summary>
		/// Проверить задачи за последние три дня с ошибками в контактах
		/// </summary>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		Task UpdateEdoTaskWithContactProblemAsync(CancellationToken stoppingToken);
	}
}
