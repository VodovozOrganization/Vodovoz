using Edo.Contracts.Xml.FormalizedDocuments;

namespace TaxcomEdoApi.Library.Services.Interfaces
{
	public interface IDocumentChecker
	{
		Format? RecognizeVersion(byte[] data);
	}
}
