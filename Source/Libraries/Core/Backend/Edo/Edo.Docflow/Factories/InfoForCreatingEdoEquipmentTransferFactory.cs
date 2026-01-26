using System;
using System.Linq;
using TaxcomEdo.Contracts.Documents;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;

namespace Edo.Docflow.Factories
{
	/// <summary>
	/// Фабрика создания информации для создания ЭДО документа "Акт приема-передачи оборудования"
	/// </summary>
	public class InfoForCreatingEdoEquipmentTransferFactory : IInfoForCreatingEdoInformalOrderDocumentFactory
	{
		private readonly ICounterpartyEdoAccountEntityController _counterpartyEdoAccountController;

		public InfoForCreatingEdoEquipmentTransferFactory(
			ICounterpartyEdoAccountEntityController counterpartyEdoAccountController
			)
		{
			_counterpartyEdoAccountController = counterpartyEdoAccountController ?? throw new ArgumentNullException(nameof(counterpartyEdoAccountController));
		}

		public InfoForCreatingEdoInformalOrderDocument CreateInfoForCreatingEdoInformalOrderDocument(OrderEntity order, OrderDocumentFileData fileData)
		{
			if(order == null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			var organizationInfo = ConvertOrganizationToOrganizationInfoForEdo(order.Contract.Organization, order.DeliveryDate.Value);
			var counterpartyInfo = ConvertCounterpartyToCounterpartyInfoForEdo(order.Client, order.Contract.Organization.Id);
			var depositSum = order.OrderItems
				.Where(oi => oi.Nomenclature != null && oi.Nomenclature.Category == NomenclatureCategory.deposit)
				.Sum(oi => Math.Round((oi.ActualCount ?? oi.Count) * oi.Price, 2));

			var data = new InfoForCreatingEdoInformalOrderDocument
			{
				MainDocumentId = Guid.NewGuid(),
				OrganizationInfoForEdo = organizationInfo,
				CounterpartyInfoForEdo = counterpartyInfo,
				Sum = depositSum,
				FileData = fileData
			};

			return data;
		}

		private EdoParticipantInfo ConvertOrganizationToOrganizationInfoForEdo(OrganizationEntity organization, DateTime dateTime)
		{
			if(organization == null)
			{
				return null;
			}

			var organizationVersion = organization.OrganizationVersions.SingleOrDefault(
				x => x.StartDate <= dateTime
					&& (x.EndDate == null || x.EndDate >= dateTime));

			return new EdoParticipantInfo
			{
				Inn = organization.INN,
				Kpp = organization.KPP,
				TaxcomEdoAccountId = organization.TaxcomEdoSettings?.EdoAccount,
				OrganizationFullName = organization.FullName
			};
		}

		private EdoParticipantInfo ConvertCounterpartyToCounterpartyInfoForEdo(CounterpartyEntity counterparty, int organizationId)
		{
			if(counterparty == null)
			{
				return null;
			}

			var counterpartyEdoAccount = _counterpartyEdoAccountController.GetDefaultCounterpartyEdoAccountByOrganizationId(counterparty, organizationId);

			return new EdoParticipantInfo
			{
				Inn = counterparty.INN,
				Kpp = counterparty.KPP,
				TaxcomEdoAccountId = counterpartyEdoAccount?.PersonalAccountIdInEdo,
				OrganizationFullName = counterparty.FullName
			};
		}

	}
}

