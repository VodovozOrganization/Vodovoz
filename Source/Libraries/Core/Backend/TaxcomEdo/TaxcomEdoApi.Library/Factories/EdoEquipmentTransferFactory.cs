using System;
using Taxcom.Client.Api.Entity;
using TaxcomEdo.Contracts.Documents;

namespace TaxcomEdoApi.Library.Factories
{
	public class EdoEquipmentTransferFactory : IEdoEquipmentTransferFactory
	{
		public NonformalizedDocument CreateEquipmentTransferDocument(InfoForCreatingEdoEquipmentTransfer data)
		{
			var orderInfo = data.OrderInfoForEdo;
			var nonformalizedDocument = new NonformalizedDocument
			{
				Number = orderInfo.Id.ToString(),
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
	}
}
