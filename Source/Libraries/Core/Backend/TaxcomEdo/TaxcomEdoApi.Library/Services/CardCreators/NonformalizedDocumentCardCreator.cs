using System.Globalization;
using System.Linq;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdo.Contracts.Xml.Container.Entities.Card;
using TaxcomEdoApi.Library.Models.Documents;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services.CardCreators
{
	public class NonformalizedDocumentCardCreator : CardCreator, ICardCreator
	{
		protected NonformalizedDocumentCardCreator(NonformalizedDocument document) : base(document)
		{
		}

		public Card CreateCard()
		{
			var card = base.CreateCardFromDocument();
			var document = Document as NonformalizedDocument;
			
			if(!string.IsNullOrWhiteSpace(document.Number))
			{
				AdditionalParameters.Add(new DescriptionAdditionalParameter
				{
					Name = "Номер",
					Value = document.Number
				});
			}

			AdditionalParameters.Add(new DescriptionAdditionalParameter
			{
				Name = "Сумма",
				Value = document.Sum.ToString(CultureInfo.InvariantCulture)
			});
			
			foreach(var additionalParameter in document.AdditionalParameter)
			{
				if(!AdditionalParameters.Any(p => p.Name == additionalParameter.Name))
				{
					AdditionalParameters.Add(additionalParameter);
				}
			}
			
			FillAdditionalData();
			
			card.Description.AdditionalData = AdditionalParameters.ToArray();
			
			return card;
		}
		
		public static ICardCreator Create(NonformalizedDocument document) => new NonformalizedDocumentCardCreator(document);
	}
}
