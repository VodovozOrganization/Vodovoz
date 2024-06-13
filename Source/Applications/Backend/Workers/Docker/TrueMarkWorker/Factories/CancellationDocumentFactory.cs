using System.Text.Json;
using TrueMark.Contracts.Documents;

namespace TrueMarkWorker.Factories
{
	public class CancellationDocumentFactory : IDocumentFactory
	{
		private readonly string _organizationInn;
		private readonly string _guid;

		public CancellationDocumentFactory(string organizationInn, string guid)
		{
			_organizationInn = organizationInn;
			_guid = guid;
		}

		public string CreateDocument()
		{
			var cancellationDocument = new DocumentCancellationDto
			{
				Inn = _organizationInn,
				LkGtinReceiptId = _guid,
				Version = 1
			};

			var serializedDocument = JsonSerializer.Serialize(cancellationDocument);

			return serializedDocument;

		}
	}
}
