using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Autofac;
using QS.Print;
using QS.Report;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Domain.Orders.Documents
{
	public class NomenclatureCertificateDocument : PrintableOrderDocument, IPrintableRDLDocument
	{
		Certificate certificate;
		[Display(Name = "Сертификат продукции")]
		public virtual Certificate Certificate {
			get => certificate;
			set => SetField(ref certificate, value, () => Certificate);
		}

		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.ProductCertificate;
		#endregion

		public override DateTime? DocumentDate => Certificate.ExpirationDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Portrait;

		public override string Name => string.Format("Сертификат продукции №{0} ({1})", Certificate.Id, Certificate.Name);

		int copiesToPrint = 1;
		public override int CopiesToPrint {
			get => copiesToPrint;
			set => copiesToPrint = value;
		}

		public virtual Dictionary<object, object> Parameters { get; set; }

		public virtual ReportInfo GetReportInfo(string connectionString = null)
		{
			var reportInfoFactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Title = String.Format("{0} от {1:d}", Name, Certificate.ExpirationDate);
			reportInfo.Identifier = "Documents.Certificate";
			reportInfo.Parameters = new Dictionary<string, object> {
				{ "certificate_id",  Certificate.Id },
				{ "order_id", Order.Id }
			};
			return reportInfo;
		}
	}
}
