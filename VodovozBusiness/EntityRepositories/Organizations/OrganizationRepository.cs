using QS.Banks.Domain;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.EntityRepositories.Organizations
{
	public class OrganizationRepository : IOrganizationRepository
	{
		public Organization GetOrganizationByInn(IUnitOfWork uow, string inn)
		{
			if(string.IsNullOrWhiteSpace(inn))
			{
				return null;
			}

			return uow.Session.QueryOver<Organization>()
				.Where(c => c.INN == inn)
				.Take(1)
				.SingleOrDefault();
		}

		public Organization GetOrganizationByAccountNumber(IUnitOfWork uow, string accountNumber)
		{
			if(string.IsNullOrWhiteSpace(accountNumber))
			{
				return null;
			}

			Account accountAlias = null;
			
			return uow.Session.QueryOver<Organization>()
				.JoinAlias(org => org.Accounts, () => accountAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where(org => accountAlias.Number == accountNumber)
				.Take(1)
				.SingleOrDefault();
		}
	}
}

