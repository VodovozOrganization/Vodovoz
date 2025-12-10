using System;
using Taxcom.Client.Api.Entity;
using TaxcomEdo.Contracts.Documents;

namespace TaxcomEdoApi.Library.Factories
{
	/// <summary>
	/// Фабрика неформальных документов заказа для ЭДО
	/// </summary>
	public class EdoInformalOrderDocumentFactory : IEdoInformalOrderDocumentFactory
	{
		public NonformalizedDocument CreateInformalOrderDocument(InfoForCreatingEdoInformalOrderDocument data)
		{
			var nonformalizedDocument = new NonformalizedDocument
			{
				Number = data.FileData.OrderId.ToString(),
				Type = DocumentType.Statement,
				Attachment = new Taxcom.Client.Api.Entity.FileData
				{
					Image = data.FileData.Image,
					Name = data.FileData.Name
				},
				Date = DateTime.Now,
				ExternalIdentifier = data.MainDocumentId.ToString(),
				Sender =
				{
					Inn = data.OrganizationInfoForEdo.Inn,
					Kpp = data.OrganizationInfoForEdo.Kpp,
					Identifier = data.OrganizationInfoForEdo.TaxcomEdoAccountId,
					Name =
					{
						Organization = data.OrganizationInfoForEdo.OrganizationFullName
					}
				},
				Recipient =
				{
					Inn = data.CounterpartyInfoForEdo.Inn,
					Kpp = data.CounterpartyInfoForEdo.Kpp,
					Identifier = data.CounterpartyInfoForEdo.TaxcomEdoAccountId,
					Name =
					{
						Organization = data.CounterpartyInfoForEdo.OrganizationFullName
					}
				},
				Sum = data.Sum
			};

			return nonformalizedDocument;
		}
	}
}
