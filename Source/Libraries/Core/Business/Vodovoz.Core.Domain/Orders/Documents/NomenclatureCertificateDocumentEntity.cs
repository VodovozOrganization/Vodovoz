using QS.Print;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class NomenclatureCertificateDocumentEntity : PrintableOrderDocumentEntity
	{
		private CertificateEntity _certificate;
		private int _copiesToPrint = 1;

		/// <summary>
		/// Сертификат продукции
		/// </summary>
		[Display(Name = "Сертификат продукции")]
		public virtual CertificateEntity Certificate
		{
			get => _certificate;
			set => SetField(ref _certificate, value);
		}

		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.ProductCertificate;
		#endregion

		public override DateTime? DocumentDate => Certificate.ExpirationDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Portrait;

		public override string Name => $"Сертификат продукции №{Certificate.Id} ({Certificate.Name})";

		public override int CopiesToPrint
		{
			get => _copiesToPrint;
			set => _copiesToPrint = value;
		}
	}
}
