using fyiReporting.RDL;
using QS.Report;
using QSProjectsLib;
using RdlEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using EmailAttachment = Mailjet.Api.Abstractions.EmailAttachment;

namespace EmailPrepareWorker.Prepares
{
	public class EmailDocumentPreparer : IEmailDocumentPreparer
	{
		public async Task<EmailAttachment> PrepareDocument(IEmailableDocument document, CounterpartyEmailType counterpartyEmailType)
		{
			bool wasHideSignature;
			ReportInfo ri;

			wasHideSignature = document.HideSignature;
			document.HideSignature = false;

			ri = document.GetReportInfo();

			document.HideSignature = wasHideSignature;

			using MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(), QSMain.ConnectionString, OutputPresentationType.PDF, true);

			string documentDate = document.DocumentDate.HasValue ? "_" + document.DocumentDate.Value.ToString("ddMMyyyy") : "";

			string fileName = counterpartyEmailType.ToString();
			switch(counterpartyEmailType)
			{
				case CounterpartyEmailType.BillDocument:
				case CounterpartyEmailType.UpdDocument:
					fileName += $"_{document.Order.Id}";
					break;
				default:
					fileName += $"_{document.Id}";
					break;
			}

			fileName += $"_{documentDate}.pdf";

			return await new ValueTask<EmailAttachment>(
				new EmailAttachment
				{
					Filename = fileName,
					ContentType = "application/pdf",
					Base64Content = Convert.ToBase64String(stream.GetBuffer())
				});
		}
	}
}
