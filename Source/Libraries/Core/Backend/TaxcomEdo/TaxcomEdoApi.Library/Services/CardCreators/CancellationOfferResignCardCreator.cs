using System.Linq;
using TaxcomEdo.Contracts.Xml.Container.Entities.Card;
using TaxcomEdoApi.Library.Models.Documents;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services.CardCreators
{
	public class CancellationOfferResignCardCreator : CardCreator, ICardCreator
	{
		protected CancellationOfferResignCardCreator(CancellationOfferResign document) : base(document)
		{
		}
		
		public Card CreateCard()
		{
			var card = base.CreateCardFromDocument();
			
			FillAdditionalData();
			
			card.Description.AdditionalData = AdditionalParameters.ToArray();
			
			return card;
		}
		
		public static ICardCreator Create(CancellationOfferResign document) =>
			new CancellationOfferResignCardCreator(document);
	}
}
