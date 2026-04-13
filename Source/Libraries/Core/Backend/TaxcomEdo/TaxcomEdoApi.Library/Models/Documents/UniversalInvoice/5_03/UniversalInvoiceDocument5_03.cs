using System;
using Core.Infrastructure;
using Edo.Contracts.Xml.FormalizedDocuments.UPD;

namespace TaxcomEdoApi.Library.Models.Documents.UniversalInvoice._5_03
{
	/// <summary>
	/// Обертка УПД 5.03
	/// </summary>
	public sealed class UniversalInvoiceDocument5_03 : UniversalInvoiceDocument
	{
		private DateTime? _date;
		
		/// <summary>
		/// Обертка XML представления УПД формата 5.03
		/// </summary>
		public UniversalTransferDocument5_03 WrapperXml { get; set; }
		
		public override string FileIdentifier =>
			WrapperXml
				.Id;
		
		public override string Number =>
			WrapperXml
				.UniversalTransferDocument
				.СвСчФакт
				.НомерДок;
		
		public override DateTime Date
		{
			get
			{
				if(_date.HasValue)
				{
					return _date.Value;
				}
				
				var date = WrapperXml.UniversalTransferDocument.СвСчФакт.ДатаДок;

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

		public override string CorrectionNumber =>
			WrapperXml
				.UniversalTransferDocument
				.СвСчФакт
				.ИспрДок?
				.НомИспр;
		
		public override string CorrectionDate => 
			WrapperXml
				.UniversalTransferDocument
				.СвСчФакт
				.ИспрДок?
				.ДатаИспр;
		
		public override decimal TotalAmountIncludingTaxes =>
			WrapperXml
				.UniversalTransferDocument
				.ТаблСчФакт
				.ВсегоОпл
				.СтТовУчНалВсего;

		public override byte[] DocumentToByteArray()
		{
			return WrapperXml.SerializeObject();
		}
	}
}
