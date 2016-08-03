using QSBanks;
using QSOrmProject;
using QSSupportLib;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;

namespace Vodovoz.Repository
{
	public static class OrganizationRepository
	{
		public const string CashOrganization = "cash_organization_id";
		public const string CashlessOrganization = "cashless_organization_id";

		public static Organization GetOrganizationByPaymentType(IUnitOfWork uow, PaymentType paymentType)
		{
			string organizationParameter="";
			switch (paymentType) {
			case PaymentType.cash:
				organizationParameter = CashOrganization;
				break;
			case PaymentType.cashless:
				organizationParameter = CashlessOrganization;
				break;
			case PaymentType.barter:
				organizationParameter = CashlessOrganization;
				break;
			}
			if (MainSupport.BaseParameters.All.ContainsKey (organizationParameter)) {
				int id = -1;
				id = int.Parse (MainSupport.BaseParameters.All [organizationParameter]);
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

