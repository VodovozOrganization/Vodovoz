using System;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdoApi.Library.Models;
using TaxcomEdoApi.Library.Models.Documents;
using FileData = TaxcomEdoApi.Library.Models.FileData;

namespace TaxcomEdoApi.Library.Factories
{
	public class EdoBillFactory : IEdoBillFactory
	{
		public NonformalizedDocument CreateBillDocument(InfoForCreatingEdoBill data)
		{
			var orderInfo = data.OrderInfoForEdo;
			var nonformalizedDocument = new NonformalizedDocument
			{
				Number = orderInfo.Id.ToString(),
				Type = DocumentType.Account,
				Attachment = new FileData
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
					Name = orderInfo.ContractInfoForEdo.OrganizationInfoForEdo.FullName
				},
				Recipient =
				{
					Inn = orderInfo.CounterpartyInfoForEdo.Inn,
					Kpp = orderInfo.CounterpartyInfoForEdo.Kpp,
					Identifier = orderInfo.CounterpartyInfoForEdo.PersonalAccountIdInEdo,
					Name = orderInfo.CounterpartyInfoForEdo.FullName
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
				Attachment = new FileData
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
					Name = organization.FullName
				},
				Recipient =
				{
					Inn = orderWithoutShipmentInfo.CounterpartyInfoForEdo.Inn,
					Kpp = orderWithoutShipmentInfo.CounterpartyInfoForEdo.Kpp,
					Identifier = orderWithoutShipmentInfo.CounterpartyInfoForEdo.PersonalAccountIdInEdo,
					Name = orderWithoutShipmentInfo.CounterpartyInfoForEdo.FullName
				},
				Sum = orderWithoutShipmentInfo.Sum
			};

			return nonformalizedDocument;
		}
	}
}
