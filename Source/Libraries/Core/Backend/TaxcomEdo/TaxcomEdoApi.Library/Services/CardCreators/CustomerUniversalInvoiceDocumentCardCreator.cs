using System.Linq;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdo.Contracts.Xml.Container.Entities.Card;
using TaxcomEdoApi.Library.Models.Documents.CustomerInvoice;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services.CardCreators
{
	public class CustomerUniversalInvoiceDocumentCardCreator : CardCreator, ICardCreator
	{
		protected CustomerUniversalInvoiceDocumentCardCreator(CustomerUniversalInvoiceDocument document) : base(document)
		{
			
		}
		
		public Card CreateCard()
		{
			var card = base.CreateCardFromDocument();
			var document = Document as CustomerUniversalInvoiceDocument;
			
			AdditionalParameters.Add(
				new DescriptionAdditionalParameter
				{
					Name = "Номер",
					Value = !string.IsNullOrEmpty(document.Number)
						? document.Number
						: string.Empty
				});
			
			FillAdditionalData();
			
			card.Description.AdditionalData = AdditionalParameters.ToArray();
			
			return card;
		}
		
		public static ICardCreator Create(CustomerUniversalInvoiceDocument document)
			=>  new CustomerUniversalInvoiceDocumentCardCreator(document);
	}
}
