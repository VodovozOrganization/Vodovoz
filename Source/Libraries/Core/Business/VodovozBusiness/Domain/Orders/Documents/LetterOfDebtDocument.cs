using System;
using System.Collections.Generic;
using Autofac;
using QS.Print;
using QS.Report;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Domain.Orders.Documents
{
	/// <summary>
	/// Документ письма о задолженности
	/// </summary>
	public class LetterOfDebtDocument : PrintableOrderDocument, IPrintableRDLDocument, ISignableDocument
	{
		private int _copiesToPrint = 2;

		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.LetterOfDebt;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo(string connectionString = null)
		{
			var reportInfoFactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Title = Name;
			reportInfo.Identifier = "Documents.LetterOfDebt";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "order_id",  Order.Id },
				{ "hide_signature", HideSignature }
			};
			return reportInfo;
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => "Письмо о задолженности";

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Portrait;

		public override int CopiesToPrint {
			get => _copiesToPrint;
			set => _copiesToPrint = value;
		}

		public virtual bool HideSignature { get; set; } = true;
	}
}

