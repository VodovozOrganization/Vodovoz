using TaxcomEdoApi.Library.Builders.ContainerDocumentsBuilders;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;
using TaxcomEdoApi.Library.Models.Documents;
using TaxcomEdoApi.Library.Services.CardCreators;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services.ContainerDocumentsServices
{
	public class CancellationOfferResignContainerDocumentService
	{
		private ICardCreator _cardCreator;
		
		public IContainerDocument CreateContainerDocument(CancellationOfferResign document)
		{
			_cardCreator = CancellationOfferResignCardCreator.Create(document);
			var card = _cardCreator.CreateCard();
			
			var containerDocument = CancellationOfferResignContainerDocumentBuilder.Create()
				.Card(card)
				.Build();

			return containerDocument;
		}
	}
}
