using fyiReporting.RDL;
using iTextSharp.text.pdf;
using Mailjet.Api.Abstractions;
using QS.Report;
using RdlEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace BitrixApi.Library.Services
{
	/// <inheritdoc/>
	public class EmailAttachmentsCreateService : IEmailAttachmentsCreateService
	{
		private const string _revisionReportIdentifier = "Client.Revision";
		private const string _notPaidOrdersBillReportIdentifier = "Documents.Bill";
		private const string _generalBillReportIdentifier = "Documents.GeneralBill";
		private const string _letterOfClaimIdentifier = "Documents.LetterOfClaim";
		private const string _orderWithoutShipmentForDebtIdentifier = "Documents.BillWithoutShipmentForDebt";

		private const string _revisionFileName = "Акт_сверки";
		private const string _notPaidOrdersBillFileName = "Неоплаченные_счета";
		private const string _generalBillFileName = "Общий_счет";
		private const string _letterOfClaimFileName = "Письмо_претензии";
		private const string _orderWithoutShipmentForDebtFileName = "Счет без отгрузки на долг";

		private readonly IReportInfoFactory _reportInfoFactory;

		public EmailAttachmentsCreateService(IReportInfoFactory reportInfoFactory)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
		}

		/// <inheritdoc/>
		public IEnumerable<EmailAttachment> CreateRevisionAttachments(int counterpartyId, int organizationId, DateTime? startDate = null, DateTime? endDate = null)
		{
			var reportInfo = GetRevisionReportInfo(counterpartyId, organizationId, startDate, endDate)
				?? throw new InvalidOperationException("Не удалось получить информацию по отчету акта сверки");
			var pdfBytes = CreatePdfReportBytes(reportInfo);

			return CreateEmailPdfAttachment(pdfBytes, _revisionFileName);
		}

		/// <inheritdoc/>
		public IEnumerable<EmailAttachment> CreateOrdersBillsAttachments(int counterpartyId, int organizationId, IEnumerable<int> orderIds)
		{
			var pdfs = new List<byte[]>();

			foreach(var orderId in orderIds)
			{
				var reportInfo = GetOrderBillReportInfo(orderId, organizationId)
					?? throw new InvalidOperationException("Не удалось получить информацию по счету по заказу");
				var pdfBytes = CreatePdfReportBytes(reportInfo);

				pdfs.Add(pdfBytes);
			}

			var mergedPdf = MergePdfs(pdfs);

			return CreateEmailPdfAttachment(mergedPdf, _notPaidOrdersBillFileName);
		}

		/// <inheritdoc/>
		public IEnumerable<EmailAttachment> CreateGeneralBillAttachments(int counterpartyId, int organizationId, IEnumerable<int> orderIds)
		{
			var reportInfo = GetGeneralBillReportInfo(orderIds, organizationId)
				?? throw new InvalidOperationException("Не удалось получить информацию по общему счету");
			var pdfBytes = CreatePdfReportBytes(reportInfo);

			return CreateEmailPdfAttachment(pdfBytes, _generalBillFileName);
		}

		/// <inheritdoc/>
		public IEnumerable<EmailAttachment> CreateLetterOfClaimAttachments(int organizationId, int clientId, string debtSumFormatted, bool hideSignature = false)
		{
			var reportInfo = GetLetterOfClaimReportInfo(organizationId, clientId, debtSumFormatted, hideSignature)
				?? throw new InvalidOperationException("Не удалось получить информацию по претензионному письму");
			var pdfBytes = CreatePdfReportBytes(reportInfo);

			return CreateEmailPdfAttachment(pdfBytes, _letterOfClaimFileName);
		}

		/// <inheritdoc/>
		public IEnumerable<EmailAttachment> CreateOrderWithoutShipmentForDebtAttachments(OrderWithoutShipmentForDebt orderWithoutShipmentForDebt)
		{
			var reportInfo = GetOrderWithoutShipmentForDebtReportInfo(orderWithoutShipmentForDebt)
				?? throw new InvalidOperationException("Не удалось получить информацию по счету без отгрузки на долг");

			var pdfBytes = CreatePdfReportBytes(reportInfo);

			return CreateEmailPdfAttachment(pdfBytes, _orderWithoutShipmentForDebtFileName);
		}

		private IEnumerable<EmailAttachment> CreateEmailPdfAttachment(byte[] attachmentBytes, string fileName)
		{
			var attachments = new List<EmailAttachment>();

			if(attachmentBytes != null)
			{
				attachments.Add(new EmailAttachment
				{
					Filename = $"{fileName}.pdf",
					Base64Content = Convert.ToBase64String(attachmentBytes)
				});
			}

			return attachments;
		}

		private byte[] CreatePdfReportBytes(ReportInfo reportInfo)
		{
			using(var stream = ReportExporter.ExportToMemoryStream(
				reportInfo.GetReportUri(),
				reportInfo.GetParametersString(),
				reportInfo.ConnectionString,
				OutputPresentationType.PDF,
				true))
			{
				return stream.GetBuffer();
			}
		}

		private ReportInfo GetRevisionReportInfo(int counterpartyId, int organizationId, DateTime? startDate = null, DateTime? endDate = null)
		{
			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = _revisionFileName;
			reportInfo.Identifier = _revisionReportIdentifier;
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "StartDate", startDate ?? DateTime.MinValue },
				{ "EndDate", endDate ?? DateTime.MaxValue },
				{ "CounterpartyId", counterpartyId },
				{ "OrganizationId", organizationId }
			};
			return reportInfo;
		}

		private ReportInfo GetOrderBillReportInfo(int orderId, int organizationId, bool hideSignature = false)
		{
			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = _notPaidOrdersBillFileName;
			reportInfo.Identifier = _notPaidOrdersBillReportIdentifier;
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "order_id", orderId },
				{ "hide_signature", hideSignature },
				{ "organization_id", organizationId }
			};
			return reportInfo;
		}

		private ReportInfo GetGeneralBillReportInfo(IEnumerable<int> ordersIds, int organizationId, bool hideSignature = false)
		{
			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = _generalBillFileName;
			reportInfo.Identifier = _generalBillReportIdentifier;
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "order_id", ordersIds },
				{ "hide_signature", hideSignature },
				{ "organization_id", organizationId }
			};
			return reportInfo;
		}

		private ReportInfo GetLetterOfClaimReportInfo(int organizationId, int clientId, string debtSumFormatted, bool hideSignature = false)
		{
			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = _letterOfClaimFileName;
			reportInfo.Identifier = _letterOfClaimIdentifier;
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "organization_id", organizationId },
				{ "client_id", clientId },
				{ "debt_sum_string", debtSumFormatted },
				{ "hide_signature", hideSignature },
			};
			return reportInfo;
		}

		private ReportInfo GetOrderWithoutShipmentForDebtReportInfo(OrderWithoutShipmentForDebt orderWithoutShipmentForDebt, bool hideSignature = false, bool special = false)
		{
			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = _orderWithoutShipmentForDebtFileName;
			reportInfo.Identifier = _orderWithoutShipmentForDebtIdentifier;
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "bill_ws_for_debt_id", orderWithoutShipmentForDebt.Id },
				{ "special_contract_number", orderWithoutShipmentForDebt.SpecialContractNumber },
				{ "organization_id", orderWithoutShipmentForDebt.Organization.Id },
				{ "hide_signature", hideSignature },
				{ "special", special },
				{ "author", orderWithoutShipmentForDebt.Author }
			};

			return reportInfo;
		}

		private byte[] MergePdfs(IEnumerable<byte[]> pdfs)
		{
			using(var mergedPdf = new MemoryStream())
			{
				using(var document = new iTextSharp.text.Document())
				{
					using(var copy = new PdfSmartCopy(document, mergedPdf))
					{
						document.Open();

						foreach(var pdfBytes in pdfs)
						{
							using(var reader = new PdfReader(pdfBytes))
							{
								for(int i = 1; i <= reader.NumberOfPages; i++)
								{
									copy.AddPage(copy.GetImportedPage(reader, i));
								}
							}
						}
					}
				}
				return mergedPdf.ToArray();
			}
		}
	}
}
