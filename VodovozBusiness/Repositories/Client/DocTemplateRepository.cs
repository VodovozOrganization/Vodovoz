using System.Collections.Generic;
using System.Linq;
using QSDocTemplates;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;

namespace Vodovoz.Repository.Client
{
	public static class DocTemplateRepository
	{
		public static ContractType GetContractTypeForPaymentType(PersonType clientType, PaymentType paymentType)
		{
			switch(paymentType) {
				case PaymentType.cash:
				case PaymentType.ByCard:
					if(clientType == PersonType.legal) {
						return ContractType.CashUL;
					}else {
						return ContractType.CashFL;
					}
				case PaymentType.BeveragesWorld:
					if(clientType == PersonType.legal) {
						return ContractType.CashBeveragesUL;
					} else {
						return ContractType.CashBeveragesFL;
					}
				case PaymentType.cashless:
				case PaymentType.ContractDoc:
					return ContractType.Cashless;
				case PaymentType.barter:
					return ContractType.Barter;
				default:
					return ContractType.Cashless;
			}
		}

		/// <summary>
		/// Получаем первый подходящий шаболон документа по указанным критериям.
		/// </summary>
		public static DocTemplate GetTemplate (IUnitOfWork uow, TemplateType type, Organization org, ContractType contractType)
		{
			var templates = uow.Session.QueryOver<DocTemplate>()
				.Where(x => x.TemplateType == type)
				.Where(x => x.Organization == org)
				.Where(x => x.ContractType == contractType)
				.List<DocTemplate>(); ;
			return templates.FirstOrDefault();
		}

		public static IList<IDocTemplate> GetAvailableTemplates(IUnitOfWork uow, TemplateType type, Organization org)
		{
			return uow.Session.QueryOver<DocTemplate>()
				.Where(x => x.TemplateType == type)
				.Where(x => x.Organization == org)
				      .List<DocTemplate>().OfType<IDocTemplate>().ToList();
		}

		public static DocTemplate GetFirstAvailableTemplate(IUnitOfWork uow, TemplateType type, Organization org)
		{
			return uow.Session.QueryOver<DocTemplate>()
				.Where(x => x.TemplateType == type)
				.Where(x => x.Organization == org)
				.List<DocTemplate>().FirstOrDefault();
		}

	}
}

