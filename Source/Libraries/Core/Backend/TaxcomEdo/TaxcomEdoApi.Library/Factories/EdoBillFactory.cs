using System;
using Taxcom.Client.Api.Entity;
using TaxcomEdo.Contracts.Documents;

namespace TaxcomEdoApi.Library.Factories
{
	public class EdoBillFactory : IEdoBillFactory
	{
		public NonformalizedDocument CreateBillDocument(InfoForCreatingEdoBill data)
		{
			var orderInfo = data.OrderInfoForEdo;
			var nonformalizedDocument = new NonformalizedDocument
			{
				Number = orderInfo.StringNumber,
				Type = DocumentType.Account,
				Attachment = new Taxcom.Client.Api.Entity.FileData
				{
					Image = data.FileData.Image,
					Name = data.FileData.Name
				},
				Date = DateTime.Now,
				ExternalIdentifier = data.MainDocumentId.ToString(),
				Sender =
				{
					Inn = orderInfo.ContractInfoForEdo.OrganizationInfoForEdo.Inn,
					Kpp = orderInfo.ContractInfoForEdo.OrganizationInfoForEdo.Kpp,
					Identifier = orderInfo.ContractInfoForEdo.OrganizationInfoForEdo.TaxcomEdoAccountId,
					Name =
					{
						Organization = orderInfo.ContractInfoForEdo.OrganizationInfoForEdo.FullName
					}
				},
				Recipient =
				{
					Inn = orderInfo.CounterpartyInfoForEdo.Inn,
					Kpp = orderInfo.CounterpartyInfoForEdo.Kpp,
					Identifier = orderInfo.CounterpartyInfoForEdo.PersonalAccountIdInEdo,
					Name =
					{
						Organization = orderInfo.CounterpartyInfoForEdo.FullName
					}
				},
				Sum = orderInfo.OrderSum
			};

			return nonformalizedDocument;
		}

		public NonformalizedDocument CreateBillWithoutShipment(InfoForCreatingBillWithoutShipmentEdo data)
		{
			var orderWithoutShipmentInfo = data.OrderWithoutShipmentInfo;
			var organization = orderWithoutShipmentInfo.OrganizationInfoForEdo;
			var nonformalizedDocument = new NonformalizedDocument
			{
				Number = orderWithoutShipmentInfo.BillNumber,
				Type = DocumentType.Account,
				Attachment = new Taxcom.Client.Api.Entity.FileData
				{
					Image = data.FileData.Image,
					Name = data.FileData.Name
				},
				Date = DateTime.Now,
				ExternalIdentifier = data.MainDocumentId.ToString(),
				Sender =
				{
					Inn = organization.Inn,
					Kpp = organization.Kpp,
					Identifier = organization.TaxcomEdoAccountId,
					Name =
					{
						Organization = organization.FullName
					}
				},
				Recipient =
				{
					Inn = orderWithoutShipmentInfo.CounterpartyInfoForEdo.Inn,
					Kpp = orderWithoutShipmentInfo.CounterpartyInfoForEdo.Kpp,
					Identifier = orderWithoutShipmentInfo.CounterpartyInfoForEdo.PersonalAccountIdInEdo,
					Name =
					{
						Organization = orderWithoutShipmentInfo.CounterpartyInfoForEdo.FullName
					}
				},
				Sum = orderWithoutShipmentInfo.Sum
			};

			return nonformalizedDocument;
		}
	}
}
