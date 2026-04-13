using System;
using Core.Infrastructure;
using Edo.Contracts.Xml.FormalizedDocuments;

namespace TaxcomEdoApi.Library.Models.Documents.CustomerInvoice._5_03
{
	public class CustomerUniversalInvoiceDocument5_03 : CustomerUniversalInvoiceDocument
	{
		private DateTime? _date;
		
		public CustomerInformationDocument5_03 WrapperXml { get; set; }
		
		public override string FileIdentifier =>
			WrapperXml
				.Id;
		
		public override string Number =>
			WrapperXml
				.CustomerInformation
				.DataSellerDocumentId
				.Id;
		
		public override DateTime Date
		{
			get
			{
				if(_date.HasValue)
				{
					return _date.Value;
				}
			
				var date = WrapperXml.CustomerInformation.DocumentDate;

				if(DateTime.TryParse(date, out var dateDocument))
				{
					_date = dateDocument;
				}
				else
				{
					throw new InvalidCastException($"Дата документа {date} не может быть конвертирована в DateTime.");
				}
				
				return _date.Value;
			}
		}

		public override string CorrectionNumber => null;

		public override string CorrectionDate => null;

		public override decimal TotalAmountIncludingTaxes => 0m;
		
		public override byte[] DocumentToByteArray()
		{
			return WrapperXml.SerializeObject();
		}
	}
}
