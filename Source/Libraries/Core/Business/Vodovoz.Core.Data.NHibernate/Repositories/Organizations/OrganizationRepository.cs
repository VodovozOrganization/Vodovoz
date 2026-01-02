using System;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Organizations
{
	public class OrganizationRepository : IOrganizationRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public OrganizationRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
		}

		public IEnumerable<string> GetEmailsForMailing()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var result = uow.Session.QueryOver<OrganizationEntity>()
				.Where(o => o.EmailForMailing != null && !string.IsNullOrEmpty(o.EmailForMailing))
				.Select(o => o.EmailForMailing).List<string>();

				return result;
			}
		}

		public OrganizationEntity GetOrganizationById(int id)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				return uow.Session.QueryOver<OrganizationEntity>()
					.Where(o => o.Id == id)
					.SingleOrDefault();
			}
		}

		public async Task<OrganizationEntity> GetOrganizationByIdAsync(int id)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				return await uow.Session.QueryOver<OrganizationEntity>()
					.Where(o => o.Id == id)
					.SingleOrDefaultAsync();
			}
		}
	}
}
