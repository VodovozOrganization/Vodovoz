using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Mango;
using Vodovoz.EntityRepositories.Mango;

namespace Vodovoz.Infrastructure.Persistance.Mango
{
	/// <inheritdoc cref="IDriverMangoExtensionNumberRepository"/>
	public class DriverMangoExtensionNumberRepository : IDriverMangoExtensionNumberRepository
	{
		public async Task<IReadOnlyCollection<int>> GetUsedExtensionNumbersAsync(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var currentYearStartDate = new DateTime(DateTime.Now.Year, 1, 1);

			var numbers = await uow.Session.Query<DriverMangoExtensionNumber>()
				.Where(x =>
					x.ExtensionNumber != null
					&& (x.Status == DriverMangoExtensionNumberStatus.Active || x.ActivatedAt >= currentYearStartDate))
				.Select(x => x.ExtensionNumber)
				.ToListAsync(cancellationToken);

			return numbers
				.Select(x => x.Value)
				.ToList();
		}

		public async Task<bool> HasActiveExtensionNumberAsync(IUnitOfWork uow, int driverId, CancellationToken cancellationToken)
		{
			return await uow.Session.Query<DriverMangoExtensionNumber>()
				.AnyAsync(
					x => x.DriverId == driverId && x.Status == DriverMangoExtensionNumberStatus.Active,
					cancellationToken);
		}

		public async Task<IReadOnlyList<DriverMangoExtensionNumber>> GetActiveExtensionNumbersAsync(
			IUnitOfWork uow,
			DateTime activatedBefore,
			CancellationToken cancellationToken)
		{
			return await uow.Session.Query<DriverMangoExtensionNumber>()
				.Where(x => x.Status == DriverMangoExtensionNumberStatus.Active && x.ActivatedAt < activatedBefore)
				.ToListAsync(cancellationToken);
		}

		public async Task<IReadOnlyList<DriverMangoExtensionNumber>> GetActiveExtensionNumbersByDriverIdsAsync(
			IUnitOfWork uow,
			IEnumerable<int> driverIds,
			CancellationToken cancellationToken)
		{
			var ids = driverIds?.Distinct().ToArray() ?? Array.Empty<int>();

			if(!ids.Any())
			{
				return new List<DriverMangoExtensionNumber>();
			}

			return await uow.Session.Query<DriverMangoExtensionNumber>()
				.Where(x =>
					ids.Contains(x.DriverId)
					&& x.ExtensionNumber != null
					&& x.Status == DriverMangoExtensionNumberStatus.Active)
				.ToListAsync(cancellationToken);
		}

		public async Task<DriverMangoExtensionNumber> GetByIdAsync(IUnitOfWork uow, int id, CancellationToken cancellationToken)
		{
			return await uow.Session.GetAsync<DriverMangoExtensionNumber>(id, cancellationToken);
		}
	}
}
