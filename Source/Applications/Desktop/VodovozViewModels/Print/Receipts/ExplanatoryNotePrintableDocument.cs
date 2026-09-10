using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using System.Linq;
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

		public ExplanatoryNotePrintableDocument(
			IReportInfoFactory reportInfoFactory,
			int noteId,
			int orderId,
			string content,
			DateTime createdDate,
			int? signatureId = null)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_content = content ?? string.Empty;
			_createdDate = createdDate;
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

			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = Name;
			reportInfo.Identifier = ReportIdentifier;
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "header", string.Join("\n", layout.HeaderLines) },
				{ "title", ReceiptCorrectionExplanatoryNoteExportHelper.DocumentTitle },
				{ "body", string.Join("\n\n", layout.BodyLines.Select(FormatBodyParagraph)) },
				{ "date_text", layout.DateText },
				{ "facsimile", ReceiptCorrectionExplanatoryNoteExportHelper.SignaturePlaceholder },
				{ "signature_id", hasSignature ? _signatureId.Value : 0 },
				{ "hide_signature", !hasSignature },
				{ "hide_facsimile", hasSignature }
			};

			return reportInfo;
		}

		/// <summary>
		/// Абзацный отступ первой строки (как в Word-шаблонах).
		/// Строки таблицы позиций не сдвигаем.
		/// </summary>
		private static string FormatBodyParagraph(string line)
		{
			if(string.IsNullOrWhiteSpace(line))
			{
				return line ?? string.Empty;
			}

			var trimmed = line.TrimStart();
			if(IsPositionsTableLine(trimmed))
			{
				return trimmed;
			}

			return "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0" + trimmed;
		}

		private static bool IsPositionsTableLine(string line)
		{
			if(string.IsNullOrEmpty(line))
			{
				return false;
			}

			if(line.IndexOf('\t') >= 0)
			{
				return true;
			}

			return line.StartsWith("№", StringComparison.Ordinal)
				|| line.StartsWith("Итого", StringComparison.OrdinalIgnoreCase)
				|| line == "-"
				|| line.StartsWith("-\t", StringComparison.Ordinal);
		}
	}
}
