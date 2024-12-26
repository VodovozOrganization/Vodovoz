using Edo.Contracts.Messages.Dto;
using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdoApi.Library.Config;

namespace TaxcomEdoApi.Library.Factories
{
	public interface IEdoUpdFactory
	{
		Fajl CreateNewUpdXml(
			InfoForCreatingEdoUpd orderInfoForEdo,
			WarrantOptions warrantOptions,
			string organizationAccountId,
			string certificateSubject);
		
		Fajl CreateNewUpdXml(
			UniversalTransferDocumentInfo updInfo,
			WarrantOptions warrantOptions,
			string organizationAccountId,
			string certificateSubject);
	}
}
