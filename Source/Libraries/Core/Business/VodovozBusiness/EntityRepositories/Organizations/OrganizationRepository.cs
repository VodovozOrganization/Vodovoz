using NHibernate.Criterion;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.Orders;
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

		public Organization GetOrganizationById(IUnitOfWork uow, int organizationId)
		{
			return uow.Session.QueryOver<Organization>()
				.Where(org => org.Id == organizationId)
				.SingleOrDefault();
		}
		
		public Organization GetPaymentFromOrganizationById(IUnitOfWork uow, int paymentFromId)
		{
			Organization organizationAlias = null;
			
			return uow.Session.QueryOver<PaymentFrom>()
				.Left.JoinAlias(pf => pf.OrganizationForOnlinePayments, () => organizationAlias)
				.Where(pf => pf.Id == paymentFromId)
				.Select(Projections.Entity(() => organizationAlias))
				.SingleOrDefault<Organization>();
		}

		public Organization GetOrganizationByTaxcomEdoAccountId(IUnitOfWork uow, string edoAccountId)
		{
			return uow.Session.QueryOver<Organization>()
				.Where(x => x.TaxcomEdoAccountId == edoAccountId)
				.SingleOrDefault();
		}

		public IList<OrganizationOwnershipType> GetOrganizationOwnershipTypeByAbbreviation(IUnitOfWork uow, string abbreviation)
		{
			return uow.Session.QueryOver<OrganizationOwnershipType>()
				.Where(o => o.Abbreviation == abbreviation)
				.List<OrganizationOwnershipType>();
		}

		public IList<OrganizationOwnershipType> GetAllOrganizationOwnershipTypes(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<OrganizationOwnershipType>()
				.List<OrganizationOwnershipType>();
		}
	}
}

