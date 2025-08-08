using fyiReporting.RDL;
using QS.DocTemplates;
using QS.DomainModel.UoW;
using QS.Report;
using RdlEngine;
using System;
using System.IO;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories.Counterparties;
using EmailAttachment = Mailjet.Api.Abstractions.EmailAttachment;

namespace EmailPrepareWorker.Prepares
{
	public class EmailDocumentPreparer : IEmailDocumentPreparer
	{
		private readonly IDocTemplateRepository _docTemplateRepository;

		public EmailDocumentPreparer(IDocTemplateRepository docTemplateRepository)
		{
			_docTemplateRepository = docTemplateRepository ?? throw new ArgumentNullException(nameof(docTemplateRepository));
		}

		public EmailAttachment PrepareDocument(IEmailableDocument document, CounterpartyEmailType counterpartyEmailType, string connectionString)
		{
			bool wasHideSignature;
			ReportInfo ri;

			wasHideSignature = document.HideSignature;
			document.HideSignature = false;

			ri = document.GetReportInfo(connectionString);

			document.HideSignature = wasHideSignature;

			using MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(), connectionString, OutputPresentationType.PDF, true);

			string documentDate = document.DocumentDate.HasValue ? "_" + document.DocumentDate.Value.ToString("ddMMyyyy") : "";

			string fileName = counterpartyEmailType.ToString();

			fileName += counterpartyEmailType switch
			{
				CounterpartyEmailType.BillDocument or CounterpartyEmailType.UpdDocument => $"_{document.Order.Id}",
				_ => $"_{document.Id}",
			};

			fileName += $"_{documentDate}.pdf";

			return new EmailAttachment
			{
				Filename = fileName,
				ContentType = "application/pdf",
				Base64Content = Convert.ToBase64String(stream.ToArray())
			};
		}

		public EmailAttachment PrepareOfferAgreementDocument(IUnitOfWork unitOfWork, CounterpartyContract contract, string connectionString)
		{
			using var fileWorker = new FileWorker();

			if(contract.DocumentTemplate is null
				&& !contract.UpdateContractTemplate(unitOfWork, _docTemplateRepository))
			{
				return null;
			}

			contract.DocumentTemplate.DocParser.SetDocObject(contract);

			var renderedFilePath = fileWorker.PrepareToExportODT(contract.DocumentTemplate, FileEditMode.Document);

			var content = Convert.ToBase64String(File.ReadAllBytes(renderedFilePath));

			return new EmailAttachment
			{
				Filename = "Договор оферты.odt",
				ContentType = "application/vnd.oasis.opendocument.text",
				Base64Content = content
			};
		}
	}
}
