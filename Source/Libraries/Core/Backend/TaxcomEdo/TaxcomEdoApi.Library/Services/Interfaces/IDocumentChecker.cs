using Edo.Contracts.Xml.Documents.FormalizedDocuments;

namespace TaxcomEdoApi.Library.Services.Interfaces
{
	public interface IDocumentChecker
	{
		Format? RecognizeVersion(byte[] data);
	}
}
