using System.Globalization;
using System.Linq;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdo.Contracts.Xml.Container.Entities.Card;
using TaxcomEdoApi.Library.Models.Documents.UniversalInvoice;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services.CardCreators
{
	public class UniversalInvoiceDocumentCardCreator : CardCreator, ICardCreator
	{
		protected UniversalInvoiceDocumentCardCreator(UniversalInvoiceDocument document) : base(document) { }
		
		public Card CreateCard()
		{
			var card = base.CreateCardFromDocument();
			var document = Document as UniversalInvoiceDocument;
			
			AdditionalParameters.Add(
				new DescriptionAdditionalParameter
				{
					Name = "Сумма",
					Value = document.TotalAmountIncludingTaxes.ToString(CultureInfo.InvariantCulture)
				});
			
			AdditionalParameters.Add(
				new DescriptionAdditionalParameter
				{
					Name = "Номер",
					Value = !string.IsNullOrEmpty(document.Number)
						? document.Number
						: string.Empty
				});
			
			AdditionalParameters.Add(
				new DescriptionAdditionalParameter
				{
					Name = "ДатаСчФ",
					Value = document.Date.ToShortDateString()
				});
			
			AdditionalParameters.Add(
				new DescriptionAdditionalParameter
				{
					Name = "НомИспрСчФ",
					Value = !string.IsNullOrEmpty(document.CorrectionNumber)
						? document.CorrectionNumber
						: string.Empty
				});
			
			AdditionalParameters.Add(
				new DescriptionAdditionalParameter
				{
					Name = "ДатаИспрСчФ",
					Value = !string.IsNullOrEmpty(document.CorrectionDate)
						? document.CorrectionDate
						: string.Empty
				});
			
			FillAdditionalData();
			
			card.Description.AdditionalData = AdditionalParameters.ToArray();
			
			return card;
		}
		
		public static ICardCreator Create(UniversalInvoiceDocument document) => 
			new UniversalInvoiceDocumentCardCreator(document);
	}
}
