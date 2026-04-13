using TaxcomEdoApi.Library.Builders.ContainerDocumentsBuilders;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;
using TaxcomEdoApi.Library.Models.Documents.CustomerInvoice;
using TaxcomEdoApi.Library.Services.CardCreators;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services.ContainerDocumentsServices
{
	public class CustomerUniversalInvoiceContainerDocumentService
	{
		private ICardCreator _cardCreator;
		
		public IContainerDocument CreateContainerDocument(CustomerUniversalInvoiceDocument document)
		{
			_cardCreator = CustomerUniversalInvoiceDocumentCardCreator.Create(document);
			var card = _cardCreator.CreateCard();
			
			var containerDocument = CustomerUniversalInvoiceContainerDocumentBuilder.Create()
				.MainFile(document)
				.Attachment(document.DataImage)
				.Card(card)
				.Build();

			return containerDocument;
		}
	}
}
