using Edo.Contracts.Messages.Dto;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdoApi.Library.Config;

namespace TaxcomEdoApi.Library.Factories.Format5_01
{
	public interface IEdoTaxcomDocumentsFactory5_01
	{
		Taxcom.Client.Api.Document.DocumentByFormat1115131.Fajl CreateNewUpdXml5_01(
			InfoForCreatingEdoUpd orderInfoForEdo,
			WarrantOptions warrantOptions,
			string organizationAccountId,
			string certificateSubject);
		
		Taxcom.Client.Api.Document.DocumentByFormat1115131.Fajl CreateNewUpdXml5_01(
			UniversalTransferDocumentInfo updInfo,
			WarrantOptions warrantOptions,
			string organizationAccountId,
			string certificateSubject);
	}
}
