using TaxcomEdoApi.Library.Builders.ContainerDocumentsBuilders;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;
using TaxcomEdoApi.Library.Models.Documents;
using TaxcomEdoApi.Library.Services.CardCreators;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services.ContainerDocumentsServices
{
	public class CancellationOfferContainerDocumentService
	{
		private ICardCreator _cardCreator;
		
		public IContainerDocument CreateContainerDocument(CancellationOfferDocument document)
		{
			_cardCreator = CancellationOfferCardCreator.Create(document);
			var card = _cardCreator.CreateCard();
			
			var containerDocument = CancellationOfferContainerDocumentBuilder.Create()
				.MainFile(document)
				.Card(card)
				.Attachment(document.Attachment)
				.Build();

			return containerDocument;
		}
	}
}
