using Vodovoz.Domain;
using QSOrmProject;
using QSSupportLib;

namespace Vodovoz.Repository
{
	public static class OrganizationRepository
	{
		const string cashOrganization = "cash_organization_id";
		const string cashlessOrganization = "cashless_organization_id";

		public static Organization GetCashOrganization (IUnitOfWork uow)
		{
			if (MainSupport.BaseParameters.All.ContainsKey (cashOrganization)) {
				int id = -1;
				id = int.Parse (MainSupport.BaseParameters.All [cashOrganization]);
				if (id == -1)
					return null;
				return uow.GetById<Organization> (id);
			}
			return null;
		}

		public static Organization GetCashlessOrganization (IUnitOfWork uow)
		{
			if (MainSupport.BaseParameters.All.ContainsKey (cashlessOrganization)) {
				int id = -1;
				id = int.Parse (MainSupport.BaseParameters.All [cashlessOrganization]);
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
	}
}

