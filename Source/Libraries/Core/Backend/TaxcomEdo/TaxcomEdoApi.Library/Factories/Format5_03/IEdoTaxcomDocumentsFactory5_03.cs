using Edo.Contracts.Messages.Dto;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdoApi.Library.Config;

namespace TaxcomEdoApi.Library.Factories.Format5_03
{
	public interface IEdoTaxcomDocumentsFactory5_03
	{
		Taxcom.Client.Api.Document.DocumentByFormat1115131_5_03.Fajl CreateUpdXml5_03(
			InfoForCreatingEdoUpd orderInfoForEdo,
			WarrantOptions warrantOptions,
			string organizationAccountId,
			string certificateSubject);
		
		Taxcom.Client.Api.Document.DocumentByFormat1115131_5_03.Fajl CreateUpdXml5_03(
			UniversalTransferDocumentInfo updInfo,
			WarrantOptions warrantOptions,
			string organizationAccountId,
			string certificateSubject);
	}
}
