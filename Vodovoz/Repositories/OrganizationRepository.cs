using Vodovoz.Domain;
using QSOrmProject;

namespace Vodovoz.Repository
{
	public static class OrganizationRepository
	{
		public static Organization GetCashOrganization (IUnitOfWork uow)
		{
			return uow.GetById<Organization> (12);
		}

		public static Organization GetCashlessOrganization (IUnitOfWork uow)
		{
			return uow.GetById<Organization> (11);
		}
	}
}

