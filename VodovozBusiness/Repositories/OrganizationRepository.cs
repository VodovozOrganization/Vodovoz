using System.Linq;
using QSBanks;
using QSOrmProject;
using QSSupportLib;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Repository.Client;

namespace Vodovoz.Repository
{
	public static class OrganizationRepository
	{
		public const string CashOrganization = "cash_organization_id";
		public const string CashlessOrganization = "cashless_organization_id";

		public static Organization GetOrganizationByPaymentType(IUnitOfWork uow, PersonType personType, PaymentType paymentType)
		{
			var contractType = DocTemplateRepository.GetContractTypeForPaymentType(personType, paymentType);

			DocTemplate template =
				uow.Session.QueryOver<DocTemplate>()
				   .Where(x => x.TemplateType == TemplateType.Contract)
				   .Where(x => x.ContractType == contractType)
				   .List().FirstOrDefault();
			if(template == null) {
				return null;
			}
			return template.Organization;
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

		public static Organization GetCashlessOrganization(IUnitOfWork uow)
		{
			if(MainSupport.BaseParameters.All.ContainsKey(CashlessOrganization)){
				return uow.GetById<Organization>(int.Parse(MainSupport.BaseParameters.All[CashlessOrganization]));
			}
			return null;
		}
	}
}

