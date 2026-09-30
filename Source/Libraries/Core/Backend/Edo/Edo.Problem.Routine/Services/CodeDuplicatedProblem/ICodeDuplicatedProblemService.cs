using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Edo.Problem.Routine.Services.CodeDuplicatedProblem
{
	/// <summary>
	/// Сервис обработки проблем с дубликатом кода в ЭДО
	/// </summary>
	public interface ICodeDuplicatedProblemService
	{
		/// <summary>
		/// Обработчик задач с дубликатом кода в ЭДО
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork</param>
		/// <param name="minEdoTaskCreationTime">Минимальное время создания задачи ЭДО</param>
		/// <param name="cancellationToken">Токен отмены</param>
		Task ProcessProblemTasksAsync(IUnitOfWork unitOfWork, DateTime minEdoTaskCreationTime, CancellationToken cancellationToken);
	}
}