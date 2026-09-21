using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Application.Receipts.Correction;
using Vodovoz.Core.Domain.Receipts;
using Vodovoz.ViewModels.Journals.JournalViewModels.Receipts;

namespace Vodovoz.ViewModels.Print.Receipts
{
	public class ExplanatoryNotePrintableDocument : IPrintableRDLDocument
	{
		public const string ReportIdentifier = "Documents.ExplanatoryNote";

		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly string _content;
		private readonly DateTime _createdDate;
		private readonly int? _signatureId;
		private readonly ReceiptCorrectionExplanatoryNoteTemplateType _templateType;

		public ExplanatoryNotePrintableDocument(
			IReportInfoFactory reportInfoFactory,
			int noteId,
			int orderId,
			string content,
			DateTime createdDate,
			ReceiptCorrectionExplanatoryNoteTemplateType templateType,
			int? signatureId = null)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_content = content ?? string.Empty;
			_createdDate = createdDate;
			_templateType = templateType;
			_signatureId = signatureId;
			NoteId = noteId;
			OrderId = orderId;
			Name = $"Пояснительная записка №{noteId} (заказ {orderId})";
			CopiesToPrint = 1;
		}

		public int NoteId { get; }

		public int OrderId { get; }

		public string Name { get; }

		public PrinterType PrintType => PrinterType.RDL;

		public DocumentOrientation Orientation => DocumentOrientation.Portrait;

		public int CopiesToPrint { get; set; }

		public Dictionary<object, object> Parameters { get; set; } = new Dictionary<object, object>();

		public ReportInfo GetReportInfo(string connectionString = null)
		{
			var layout = ReceiptCorrectionExplanatoryNoteExportHelper.ParseNoteLayout(_content, _createdDate);
			var hasSignature = _signatureId.HasValue && _signatureId.Value > 0;
			var hasPositions = ReceiptCorrectionExplanatoryNoteBuilder.TemplateHasPositions(_templateType)
				|| layout.HasPositions;

			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = Name;
			reportInfo.Identifier = ReportIdentifier;
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "note_id", NoteId },
				{ "header", string.Join("\n", layout.HeaderLines) },
				{ "title", ReceiptCorrectionExplanatoryNoteExportHelper.DocumentTitle },
				{ "body", BuildBodyParameter(layout) },
				{ "date_text", layout.DateText },
				{ "total_text", layout.TotalText ?? string.Empty },
				{ "facsimile", ReceiptCorrectionExplanatoryNoteExportHelper.SignaturePlaceholder },
				{ "signature_id", hasSignature ? _signatureId.Value : 0 },
				{ "hide_signature", !hasSignature },
				{ "hide_facsimile", hasSignature },
				{ "hide_positions", !hasPositions },
				{ "hide_standalone_footer", hasPositions }
			};

			return reportInfo;
		}

		private static string BuildBodyParameter(ReceiptCorrectionExplanatoryNoteExportHelper.NoteLayout layout)
		{
			return string.Join("\n\n", layout.BodyLines.Select(FormatBodyParagraph));
		}

		/// <summary>
		/// Абзацный отступ первой строки (как в Word-шаблонах).
		/// </summary>
		private static string FormatBodyParagraph(string line)
		{
			if(string.IsNullOrWhiteSpace(line))
			{
				return line ?? string.Empty;
			}

			return "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0" + line.TrimStart();
		}
	}
}
