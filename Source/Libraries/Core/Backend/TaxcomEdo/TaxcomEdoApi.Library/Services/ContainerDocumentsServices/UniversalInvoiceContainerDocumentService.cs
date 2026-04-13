using TaxcomEdoApi.Library.Builders.ContainerDocumentsBuilders;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;
using TaxcomEdoApi.Library.Models.Documents.UniversalInvoice;
using TaxcomEdoApi.Library.Services.CardCreators;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services.ContainerDocumentsServices
{
	public class UniversalInvoiceContainerDocumentService
	{
		private ICardCreator _cardCreator;
		
		public IContainerDocument CreateContainerDocument(UniversalInvoiceDocument document)
		{
			_cardCreator = UniversalInvoiceDocumentCardCreator.Create(document);
			var card = _cardCreator.CreateCard();
			
			var containerDocument = UniversalInvoiceContainerDocumentBuilder.Create()
				.MainFile(document)
				.Attachment(document)
				.Card(card)
				.Build();

			return containerDocument;
		}
	}
}
