using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Mango;

namespace Vodovoz.EntityRepositories.Mango
{
	/// <summary>
	/// Репозиторий заявок на регистрацию водителей как сотрудников Манго
	/// </summary>
	public interface IDriverMangoEmployeeRegistrationRequestRepository
	{
		/// <summary>
		/// Возвращает идентификаторы новых (необработанных) заявок
		/// </summary>
		Task<IReadOnlyList<int>> GetNewRequestIdsAsync(IUnitOfWork uow, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает заявку по идентификатору
		/// </summary>
		Task<DriverMangoEmployeeRegistrationRequest> GetByIdAsync(IUnitOfWork uow, int id, CancellationToken cancellationToken);
	}
}
