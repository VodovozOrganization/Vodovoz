using System;
using System.Linq;
using Taxcom.Client.Api.Entity;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Data.Orders.OrdersWithoutShipment;

namespace TaxcomEdoApi.Factories
{
	public class EdoBillFactory : IEdoBillFactory
	{
		public NonformalizedDocument CreateBillDocument(Order order, byte[] attachmentFile, string attachmentName)
		{
			var nonformalizedDocument = new NonformalizedDocument
			{
				Number = order.Id.ToString(),
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
					Inn = order.Contract.Organization.INN,
					Kpp = order.Contract.Organization.KPP,
					Identifier = order.Contract.Organization.TaxcomEdoAccountId,
					Name = { Organization = order.Contract.Organization.FullName }
				},
				Recipient =
				{
					Inn = order.Counterparty.INN,
					Kpp = order.Counterparty.KPP,
					Identifier = order.Counterparty.PersonalAccountIdInEdo,
					Name = { Organization = order.Counterparty.FullName }
				},
				Sum = order.OrderSum
			};

			return nonformalizedDocument;
		}

		public NonformalizedDocument CreateBillWithoutShipment(
			OrderWithoutShipmentInfo orderWithoutShipmentInfo, byte[] attachmentFile, string attachmentName)
		{
			var organization = orderWithoutShipmentInfo.Organization;
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
					Inn = orderWithoutShipmentInfo.Counterparty.INN,
					Kpp = orderWithoutShipmentInfo.Counterparty.KPP,
					Identifier = orderWithoutShipmentInfo.Counterparty.PersonalAccountIdInEdo,
					Name = { Organization = orderWithoutShipmentInfo.Counterparty.FullName }
				},
				Sum = orderWithoutShipmentInfo.Sum
			};

			return nonformalizedDocument;
		}
	}
}
