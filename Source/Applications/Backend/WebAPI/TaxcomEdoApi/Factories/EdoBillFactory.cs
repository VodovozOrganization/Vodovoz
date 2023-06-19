using System;
using Taxcom.Client.Api.Entity;
using Vodovoz.Domain.Orders;
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
	}
}
