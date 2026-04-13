using Edo.Contracts.Messages.Dto;
using Edo.Contracts.Xml.FormalizedDocuments.UPD;
using TaxcomEdoApi.Library.Config;

namespace TaxcomEdoApi.Library.Factories.Format5_03
{
	public interface IEdoTaxcomDocumentsFactory5_03
	{
		UniversalTransferDocument5_03 CreateUpdXml5_03(
			UniversalTransferDocumentInfo updInfo,
			WarrantOptions warrantOptions,
			string organizationAccountId,
			string certificateSubject);
	}
}
