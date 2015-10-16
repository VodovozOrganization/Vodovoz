using Vodovoz.Domain;
using QSOrmProject;

namespace Vodovoz.Repository
{
	public static class OrganizationRepository
	{
		public const int CashOrganizationId = 12;
		public const int CashlessOrganizationId = 11;

		public static Organization GetCashOrganization (IUnitOfWork uow)
		{
			return uow.GetById<Organization> (CashOrganizationId);
		}

		public static Organization GetCashlessOrganization (IUnitOfWork uow)
		{
			return uow.GetById<Organization> (CashlessOrganizationId);
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
	}
}

