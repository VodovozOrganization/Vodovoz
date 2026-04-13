using System;
using TaxcomEdoApi.Library.Builders.ContainerDocumentsBuilders;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;
using TaxcomEdoApi.Library.Models.Documents;
using TaxcomEdoApi.Library.Models.Interfaces;
using TaxcomEdoApi.Library.Services.CardCreators;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services.ContainerDocumentsServices
{
	/// <summary>
	/// Сервис для работы с неформализованным документом
	/// </summary>
	public class NonformalizedContainerDocumentService
	{
		private ICardCreator _cardCreator;
		
		public IContainerDocument CreateContainerDocument(NonformalizedDocument document)
		{
			ThrowIfNullDocument(document);
			_cardCreator = NonformalizedDocumentCardCreator.Create(document);
			var card = _cardCreator.CreateCard();
			
			var containerDocument = NonformalizedContainerDocumentBuilder.Create()
				.Card(card)
				.Attachment(document.Attachment)
				.TransactionCode(document.TransactionCode)
				.Build();

			return containerDocument;
		}

		private void ThrowIfNullDocument(IDocument document)
		{
			if(document is null)
			{
				throw new ArgumentNullException(nameof(document));
			}
		}
	}
}
