﻿using NHibernate.Linq;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.Infrastructure.Persistance.Organizations
{
	internal sealed class OrganizationRepository : IOrganizationRepository
	{
		private readonly IOrganizationSettings _organizationSettings;

		public OrganizationRepository(IOrganizationSettings organizationSettings)
		{
			_organizationSettings = organizationSettings ?? throw new System.ArgumentNullException(nameof(organizationSettings));
		}

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

		public Organization GetOrganizationByTaxcomEdoAccountId(IUnitOfWork uow, string edoAccountId)
		{
			return (
					from organization in uow.Session.Query<Organization>()
					join taxcomEdoSettings in uow.Session.Query<TaxcomEdoSettings>()
						on organization.Id equals taxcomEdoSettings.OrganizationId
					where taxcomEdoSettings.EdoAccount == edoAccountId
					select organization)
				.FirstOrDefault();
		}

		public async Task<IList<Organization>> GetOrganizationsByTaxcomEdoAccountIds(IUnitOfWork uow, string[] edoAccountIds, CancellationToken cancellationToken)
		{
			return await (
				from organization in uow.Session.Query<Organization>()
				join taxcomEdoSettings in uow.Session.Query<TaxcomEdoSettings>()
					on organization.Id equals taxcomEdoSettings.OrganizationId
				where edoAccountIds.Contains(taxcomEdoSettings.EdoAccount)
				select organization)
				.ToListAsync(cancellationToken);
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

		public Organization GetCommonOrganisation(IUnitOfWork uow)
		{
			return uow.GetById<Organization>(_organizationSettings.CommonCashDistributionOrganisationId);
		}
	}
}

