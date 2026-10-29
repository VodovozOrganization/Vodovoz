using NHibernate.Linq;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Mango;
using Vodovoz.EntityRepositories.Mango;

namespace Vodovoz.Infrastructure.Persistance.Mango
{
	/// <inheritdoc cref="IDriverMangoEmployeeRegistrationRequestRepository"/>
	public class DriverMangoEmployeeRegistrationRequestRepository : IDriverMangoEmployeeRegistrationRequestRepository
	{
		public async Task<IReadOnlyList<int>> GetNewRequestIdsAsync(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			return await uow.Session.Query<DriverMangoEmployeeRegistrationRequest>()
				.Where(x => x.Status == DriverMangoEmployeeRegistrationRequestStatus.New)
				.Select(x => x.Id)
				.ToListAsync(cancellationToken);
		}

		public async Task<DriverMangoEmployeeRegistrationRequest> GetByIdAsync(IUnitOfWork uow, int id, CancellationToken cancellationToken)
		{
			return await uow.Session.GetAsync<DriverMangoEmployeeRegistrationRequest>(id, cancellationToken);
		}
	}
}
