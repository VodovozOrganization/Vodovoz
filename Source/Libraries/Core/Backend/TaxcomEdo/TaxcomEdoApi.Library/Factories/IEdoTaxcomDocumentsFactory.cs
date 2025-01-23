using Edo.Contracts.Messages.Dto;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdoApi.Library.Config;

namespace TaxcomEdoApi.Library.Factories
{
	public interface IEdoTaxcomDocumentsFactory
	{
		Taxcom.Client.Api.Document.DocumentByFormat1115131.Fajl CreateNewUpdXml(
			InfoForCreatingEdoUpd orderInfoForEdo,
			WarrantOptions warrantOptions,
			string organizationAccountId,
			string certificateSubject);
		
		Taxcom.Client.Api.Document.DocumentByFormat1115131.Fajl CreateNewUpdXml(
			UniversalTransferDocumentInfo updInfo,
			WarrantOptions warrantOptions,
			string organizationAccountId,
			string certificateSubject);
	}
}
