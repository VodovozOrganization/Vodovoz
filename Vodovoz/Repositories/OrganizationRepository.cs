using Vodovoz.Domain;
using QSOrmProject;
using QSSupportLib;
using System.Collections.Generic;
using QSBanks;

namespace Vodovoz.Repository
{
	public static class OrganizationRepository
	{
		public const string CashOrganization = "cash_organization_id";
		public const string CashlessOrganization = "cashless_organization_id";

		public static Organization GetCashOrganization (IUnitOfWork uow)
		{
			if (MainSupport.BaseParameters.All.ContainsKey (CashOrganization)) {
				int id = -1;
				id = int.Parse (MainSupport.BaseParameters.All [CashOrganization]);
				if (id == -1)
					return null;
				return uow.GetById<Organization> (id);
			}
			return null;
		}

		public static Organization GetCashlessOrganization (IUnitOfWork uow)
		{
			if (MainSupport.BaseParameters.All.ContainsKey (CashlessOrganization)) {
				int id = -1;
				id = int.Parse (MainSupport.BaseParameters.All [CashlessOrganization]);
				if (id == -1)
					return null;
				return uow.GetById<Organization> (id);
			}
			return null;
		}

		public static Organization GetOrganizationByName (IUnitOfWork uow, string fullName)
		{
			if (string.IsNullOrWhiteSpace (fullName))
				return null;
			return uow.Session.QueryOver<Organization> ()
				.Where (c => c.FullName == fullName)
				.Take (1)
				.SingleOrDefault ();
		}

		public static Organization GetOrganizationByInn (IUnitOfWork uow, string inn)
		{
			if (string.IsNullOrWhiteSpace (inn))
				return null;
			return uow.Session.QueryOver<Organization> ()
				.Where (c => c.INN == inn)
				.Take (1)
				.SingleOrDefault ();
		}

		public static Organization GetOrganizationByAccountNumber (IUnitOfWork uow, string accountNumber)
		{
			if (string.IsNullOrWhiteSpace (accountNumber))
				return null;
			Account accountAlias = null;
			return uow.Session.QueryOver<Organization> ()
				.JoinAlias (org => org.Accounts, () => accountAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where (org => accountAlias.Number == accountNumber)
				.Take (1)
				.SingleOrDefault ();
		}
	}
}

