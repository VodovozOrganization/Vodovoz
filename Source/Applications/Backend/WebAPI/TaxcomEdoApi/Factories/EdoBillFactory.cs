using System;
using System.Linq;
using Taxcom.Client.Api.Entity;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Data.Orders.OrdersWithoutShipment;

namespace TaxcomEdoApi.Factories
{
	public class EdoBillFactory : IEdoBillFactory
	{
		public NonformalizedDocument CreateBillDocument(OrderInfoForEdo orderInfoForEdo, byte[] attachmentFile, string attachmentName)
		{
			var nonformalizedDocument = new NonformalizedDocument
			{
				Number = orderInfoForEdo.Id.ToString(),
				Type = DocumentType.Account,
				Attachment = new FileData
				{
					Image = attachmentFile,
					Name = attachmentName
				},
				Date = DateTime.Now,
				ExternalIdentifier = Guid.NewGuid().ToString(),
				Sender =
				{
					Inn = orderInfoForEdo.ContractInfoForEdo.OrganizationInfoForEdo.INN,
					Kpp = orderInfoForEdo.ContractInfoForEdo.OrganizationInfoForEdo.KPP,
					Identifier = orderInfoForEdo.ContractInfoForEdo.OrganizationInfoForEdo.TaxcomEdoAccountId,
					Name = { Organization = orderInfoForEdo.ContractInfoForEdo.OrganizationInfoForEdo.FullName }
				},
				Recipient =
				{
					Inn = orderInfoForEdo.CounterpartyInfoForEdo.INN,
					Kpp = orderInfoForEdo.CounterpartyInfoForEdo.KPP,
					Identifier = orderInfoForEdo.CounterpartyInfoForEdo.PersonalAccountIdInEdo,
					Name = { Organization = orderInfoForEdo.CounterpartyInfoForEdo.FullName }
				},
				Sum = orderInfoForEdo.OrderSum
			};

			return nonformalizedDocument;
		}

		public NonformalizedDocument CreateBillWithoutShipment(
			OrderWithoutShipmentInfo orderWithoutShipmentInfo, byte[] attachmentFile, string attachmentName)
		{
			var organization = orderWithoutShipmentInfo.OrganizationInfoForEdo;
			var nonformalizedDocument = new NonformalizedDocument
			{
				Number = "Ф-" + orderWithoutShipmentInfo.Id.ToString(),
				Type = DocumentType.Account,
				Attachment = new FileData
				{
					Image = attachmentFile,
					Name = attachmentName
				},
				Date = DateTime.Now,
				ExternalIdentifier = Guid.NewGuid().ToString(),
				Sender =
				{
					Inn = organization.INN,
					Kpp = organization.KPP,
					Identifier = organization.TaxcomEdoAccountId,
					Name = { Organization = organization.FullName }
				},
				Recipient =
				{
					Inn = orderWithoutShipmentInfo.CounterpartyInfoForEdo.INN,
					Kpp = orderWithoutShipmentInfo.CounterpartyInfoForEdo.KPP,
					Identifier = orderWithoutShipmentInfo.CounterpartyInfoForEdo.PersonalAccountIdInEdo,
					Name = { Organization = orderWithoutShipmentInfo.CounterpartyInfoForEdo.FullName }
				},
				Sum = orderWithoutShipmentInfo.Sum
			};

			return nonformalizedDocument;
		}
	}
}
