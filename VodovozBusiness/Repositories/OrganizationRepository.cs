using System;
using System.Linq;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Repository.Client;
using Vodovoz.Services;

namespace Vodovoz.Repositories
{
	public static class OrganizationRepository
	{
		internal static Func<IUnitOfWork, PersonType, PaymentType, Organization> GetOrganizationByPaymentTypeTestGap;
		public static Organization GetOrganizationByPaymentType(IUnitOfWork uow, PersonType personType, PaymentType paymentType)
		{
			if(GetOrganizationByPaymentTypeTestGap != null)
				return GetOrganizationByPaymentTypeTestGap(uow, personType, paymentType);

			var contractType = DocTemplateRepository.GetContractTypeForPaymentType(personType, paymentType);

			DocTemplate template =
				uow.Session.QueryOver<DocTemplate>()
				   .Where(x => x.TemplateType == TemplateType.Contract)
				   .Where(x => x.ContractType == contractType)
				   .List().FirstOrDefault();
			return template?.Organization;
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

		public static Organization GetMainOrganization(IUnitOfWork uow, int id)
		{
			return uow.GetById<Organization>(id);
		}
	}
}

