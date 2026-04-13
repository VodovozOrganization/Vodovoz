using System.Linq;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdo.Contracts.Xml.Container.Entities.Card;
using TaxcomEdoApi.Library.Models.Documents;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services.CardCreators
{
	public class CancellationOfferCardCreator : CardCreator, ICardCreator
	{
		protected CancellationOfferCardCreator(CancellationOfferDocument document) : base(document)
		{
		}
		
		public Card CreateCard()
		{
			var card = base.CreateCardFromDocument();
			var document = Document as CancellationOfferDocument;
			
			if(!string.IsNullOrWhiteSpace(document.Number))
			{
				AdditionalParameters.Add(new DescriptionAdditionalParameter
				{
					Name = "Номер",
					Value = !string.IsNullOrEmpty(document.Number) ? document.Number : string.Empty
				});
			}
			
			FillAdditionalData();
			
			card.Description.AdditionalData = AdditionalParameters.ToArray();
			
			return card;
		}
		
		public static ICardCreator Create(CancellationOfferDocument document) => new CancellationOfferCardCreator(document);
	}
}
