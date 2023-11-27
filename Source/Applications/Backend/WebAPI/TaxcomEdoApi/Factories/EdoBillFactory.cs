using System;
using Taxcom.Client.Api.Entity;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;

namespace TaxcomEdoApi.Factories
{
	public class EdoBillFactory
	{
		public NonformalizedDocument CreateBillDocument(Order order, byte[] attachmentFile, string attachmentName, Organization organization)
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
					Identifier = organization.TaxcomEdoAccountId,
					Name = { Organization = organization.FullName }
				},
				Recipient =
				{
					Inn = order.Client.INN,
					Kpp = order.Client.KPP,
					Identifier = order.Client.PersonalAccountIdInEdo,
					Name = { Organization = order.Client.FullName }
				},
				Sum = order.OrderSum
			};

			return nonformalizedDocument;
		}

		public NonformalizedDocument CreateBillWithoutShipmentForAdvancePaymentDocument(OrderWithoutShipmentForAdvancePayment orderWithoutShipmentForAdvancePayment, byte[] attachmentFile, string attachmentName, Organization organization)
		{
			var nonformalizedDocument = new NonformalizedDocument
			{
				Number = orderWithoutShipmentForAdvancePayment.Order.Id.ToString(),
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
					Inn = orderWithoutShipmentForAdvancePayment.Order.Contract.Organization.INN,
					Kpp = orderWithoutShipmentForAdvancePayment.Order.Contract.Organization.KPP,
					Identifier = organization.TaxcomEdoAccountId,
					Name = { Organization = organization.FullName }
				},
				Recipient =
				{
					Inn = orderWithoutShipmentForAdvancePayment.Client.INN,
					Kpp = orderWithoutShipmentForAdvancePayment.Client.KPP,
					Identifier = orderWithoutShipmentForAdvancePayment.Client.PersonalAccountIdInEdo,
					Name = { Organization = orderWithoutShipmentForAdvancePayment.Client.FullName }
				},
				Sum = orderWithoutShipmentForAdvancePayment.Order.OrderSum
			};

			return nonformalizedDocument;
		}

		public NonformalizedDocument CreateBillWithoutShipmentForDebtDocument(OrderWithoutShipmentForDebt orderWithoutShipmentForDebt, byte[] attachmentFile, string attachmentName, Organization organization)
		{
			var nonformalizedDocument = new NonformalizedDocument
			{
				Number = orderWithoutShipmentForDebt.Order.Id.ToString(),
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
					Inn = orderWithoutShipmentForDebt.Order.Contract.Organization.INN,
					Kpp = orderWithoutShipmentForDebt.Order.Contract.Organization.KPP,
					Identifier = organization.TaxcomEdoAccountId,
					Name = { Organization = organization.FullName }
				},
				Recipient =
				{
					Inn = orderWithoutShipmentForDebt.Client.INN,
					Kpp = orderWithoutShipmentForDebt.Client.KPP,
					Identifier = orderWithoutShipmentForDebt.Client.PersonalAccountIdInEdo,
					Name = { Organization = orderWithoutShipmentForDebt.Client.FullName }
				},
				Sum = orderWithoutShipmentForDebt.Order.OrderSum
			};

			return nonformalizedDocument;
		}

		public NonformalizedDocument CreateBillWithoutShipmentForPaymentDocument(OrderWithoutShipmentForPayment orderWithoutShipmentForPayment, byte[] attachmentFile, string attachmentName, Organization organization)
		{
			var nonformalizedDocument = new NonformalizedDocument
			{
				Number = orderWithoutShipmentForPayment.Order.Id.ToString(),
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
					Inn = orderWithoutShipmentForPayment.Order.Contract.Organization.INN,
					Kpp = orderWithoutShipmentForPayment.Order.Contract.Organization.KPP,
					Identifier = organization.TaxcomEdoAccountId,
					Name = { Organization = organization.FullName }
				},
				Recipient =
				{
					Inn = orderWithoutShipmentForPayment.Client.INN,
					Kpp = orderWithoutShipmentForPayment.Client.KPP,
					Identifier = orderWithoutShipmentForPayment.Client.PersonalAccountIdInEdo,
					Name = { Organization = orderWithoutShipmentForPayment.Client.FullName }
				},
				Sum = orderWithoutShipmentForPayment.Order.OrderSum
			};

			return nonformalizedDocument;
		}
	}
}
