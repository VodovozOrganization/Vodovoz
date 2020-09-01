using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Repository.Client;

namespace Vodovoz.Repositories.Client
{
	public class ClientDocumentsRepository
	{
		/// <summary>
		/// Создает договор с заданными параметрами
		/// </summary>
		public static CounterpartyContract CreateDefaultContract(IUnitOfWork UoW, Counterparty client, PaymentType paymentType, DateTime? issueDate)
		{
			var contractType = DocTemplateRepository.GetContractTypeForPaymentType(client.PersonType, paymentType);
			CounterpartyContract result;
			using(var uow = CounterpartyContract.Create(client)) {
				var contract = uow.Root;
				var org = OrganizationRepository.GetOrganizationByPaymentType(uow, client.PersonType, paymentType);
				contract.Organization = org;
				contract.IsArchive = false;
				contract.ContractType = contractType;
				if(issueDate.HasValue) {
					contract.IssueDate = issueDate.Value;
				}
				contract.AdditionalAgreements = new List<AdditionalAgreement>();
				uow.Save();
				result = UoW.GetById<CounterpartyContract>(uow.Root.Id);
			}
			return result;
		}

		/// <summary>
		/// Создает дополнительное соглашение для доставки воды
		/// </summary>
		public static WaterSalesAgreement CreateDefaultWaterAgreement(IUnitOfWork UoW, DeliveryPoint deliveryPoint, DateTime? deliveryDate, CounterpartyContract contract)
		{
			WaterSalesAgreement result = null;
			using(var uow = WaterSalesAgreement.Create(contract)) {
				uow.Root.DeliveryPoint = deliveryPoint;
				uow.Root.FillFixedPricesFromDeliveryPoint(uow);
				if(deliveryDate.HasValue) {
					uow.Root.IssueDate = deliveryDate.Value;
					uow.Root.StartDate = deliveryDate.Value;
				}
				uow.Save();
				result = UoW.GetById<WaterSalesAgreement>(uow.Root.Id);
			}
			return result;
		}
	}
}
