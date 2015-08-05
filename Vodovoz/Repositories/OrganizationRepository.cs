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
	}
}

