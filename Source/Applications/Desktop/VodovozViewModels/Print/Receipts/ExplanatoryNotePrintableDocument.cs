using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using Vodovoz.ViewModels.Journals.JournalViewModels.Receipts;

namespace Vodovoz.ViewModels.Print.Receipts
{
	public class ExplanatoryNotePrintableDocument : IPrintableRDLDocument
	{
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly string _content;
		private readonly DateTime _createdDate;
		private readonly byte[] _signaturePng;

		public ExplanatoryNotePrintableDocument(
			IReportInfoFactory reportInfoFactory,
			int noteId,
			int orderId,
			string content,
			DateTime createdDate,
			byte[] signaturePng = null)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_content = content ?? string.Empty;
			_createdDate = createdDate;
			_signaturePng = signaturePng;
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
			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = Name;
			reportInfo.Source = BuildRdlSource(_content, _createdDate, _signaturePng);
			return reportInfo;
		}

		private static string BuildRdlSource(string content, DateTime createdDate, byte[] signaturePng)
		{
			var layout = ReceiptCorrectionExplanatoryNoteExportHelper.ParseNoteLayout(content, createdDate);
			var header = Escape(string.Join("\n", layout.HeaderLines));
			var body = Escape(string.Join("\n\n", layout.BodyLines.Select(FormatBodyParagraph)));
			var dateText = Escape(layout.DateText);
			var title = Escape(ReceiptCorrectionExplanatoryNoteExportHelper.DocumentTitle);
			var placeholder = Escape(ReceiptCorrectionExplanatoryNoteExportHelper.SignaturePlaceholder);
			var hasSignature = signaturePng != null && signaturePng.Length > 0;
			var signatureBase64 = hasSignature ? Convert.ToBase64String(signaturePng) : null;

			var builder = new StringBuilder();
			builder.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
			builder.AppendLine(@"<Report xmlns=""http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition"" xmlns:rd=""http://schemas.microsoft.com/SQLServer/reporting/reportdesigner"">");
			builder.AppendLine(@"  <PageHeight>11.69in</PageHeight>");
			builder.AppendLine(@"  <PageWidth>8.27in</PageWidth>");
			builder.AppendLine(@"  <TopMargin>0.6in</TopMargin>");
			builder.AppendLine(@"  <LeftMargin>0.7in</LeftMargin>");
			builder.AppendLine(@"  <RightMargin>0.7in</RightMargin>");
			builder.AppendLine(@"  <BottomMargin>0.6in</BottomMargin>");
			builder.AppendLine(@"  <Width>6.87in</Width>");
			builder.AppendLine(@"  <Body>");
			builder.AppendLine(@"    <Height>10.2in</Height>");
			builder.AppendLine(@"    <ReportItems>");

			// Шапка справа (как в Word-шаблонах объяснительных)
			builder.AppendLine(@"      <Textbox Name=""tbHeader"">");
			builder.AppendLine(@"        <Top>0in</Top>");
			builder.AppendLine(@"        <Left>3.6in</Left>");
			builder.AppendLine(@"        <Width>3.27in</Width>");
			builder.AppendLine(@"        <Height>1.3in</Height>");
			builder.AppendLine(@"        <CanGrow>true</CanGrow>");
			builder.AppendLine($"        <Value>{header}</Value>");
			builder.AppendLine(@"        <Style>");
			builder.AppendLine(@"          <FontFamily>Arial</FontFamily>");
			builder.AppendLine(@"          <FontSize>12pt</FontSize>");
			builder.AppendLine(@"          <TextAlign>Right</TextAlign>");
			builder.AppendLine(@"        </Style>");
			builder.AppendLine(@"      </Textbox>");

			// Заголовок по центру
			builder.AppendLine(@"      <Textbox Name=""tbTitle"">");
			builder.AppendLine(@"        <Top>1.5in</Top>");
			builder.AppendLine(@"        <Left>0in</Left>");
			builder.AppendLine(@"        <Width>6.87in</Width>");
			builder.AppendLine(@"        <Height>0.35in</Height>");
			builder.AppendLine($"        <Value>{title}</Value>");
			builder.AppendLine(@"        <Style>");
			builder.AppendLine(@"          <FontFamily>Arial</FontFamily>");
			builder.AppendLine(@"          <FontSize>14pt</FontSize>");
			builder.AppendLine(@"          <FontWeight>Bold</FontWeight>");
			builder.AppendLine(@"          <TextAlign>Center</TextAlign>");
			builder.AppendLine(@"        </Style>");
			builder.AppendLine(@"      </Textbox>");

			// Текст объяснительной
			builder.AppendLine(@"      <Textbox Name=""tbBody"">");
			builder.AppendLine(@"        <Top>2.0in</Top>");
			builder.AppendLine(@"        <Left>0in</Left>");
			builder.AppendLine(@"        <Width>6.87in</Width>");
			builder.AppendLine(@"        <Height>5.8in</Height>");
			builder.AppendLine(@"        <CanGrow>true</CanGrow>");
			builder.AppendLine($"        <Value>{body}</Value>");
			builder.AppendLine(@"        <Style>");
			builder.AppendLine(@"          <FontFamily>Arial</FontFamily>");
			builder.AppendLine(@"          <FontSize>12pt</FontSize>");
			builder.AppendLine(@"          <TextAlign>Justify</TextAlign>");
			builder.AppendLine(@"        </Style>");
			builder.AppendLine(@"      </Textbox>");

			// Дата слева
			builder.AppendLine(@"      <Textbox Name=""tbDate"">");
			builder.AppendLine(@"        <Top>8.1in</Top>");
			builder.AppendLine(@"        <Left>0in</Left>");
			builder.AppendLine(@"        <Width>2.5in</Width>");
			builder.AppendLine(@"        <Height>0.3in</Height>");
			builder.AppendLine($"        <Value>{dateText}</Value>");
			builder.AppendLine(@"        <Style>");
			builder.AppendLine(@"          <FontFamily>Arial</FontFamily>");
			builder.AppendLine(@"          <FontSize>12pt</FontSize>");
			builder.AppendLine(@"          <TextAlign>Left</TextAlign>");
			builder.AppendLine(@"        </Style>");
			builder.AppendLine(@"      </Textbox>");

			if(hasSignature)
			{
				builder.AppendLine(@"      <Image Name=""imgSignature"">");
				builder.AppendLine(@"        <Top>7.9in</Top>");
				builder.AppendLine(@"        <Left>3.5in</Left>");
				builder.AppendLine(@"        <Width>3.2in</Width>");
				builder.AppendLine(@"        <Height>1.4in</Height>");
				builder.AppendLine(@"        <Source>Embedded</Source>");
				builder.AppendLine(@"        <Value>SignatureImg</Value>");
				builder.AppendLine(@"        <MIMEType>image/png</MIMEType>");
				builder.AppendLine(@"        <Sizing>FitProportional</Sizing>");
				builder.AppendLine(@"      </Image>");
			}
			else
			{
				builder.AppendLine(@"      <Textbox Name=""tbFacsimile"">");
				builder.AppendLine(@"        <Top>8.1in</Top>");
				builder.AppendLine(@"        <Left>3.5in</Left>");
				builder.AppendLine(@"        <Width>3.37in</Width>");
				builder.AppendLine(@"        <Height>0.3in</Height>");
				builder.AppendLine($"        <Value>{placeholder}</Value>");
				builder.AppendLine(@"        <Style>");
				builder.AppendLine(@"          <FontFamily>Arial</FontFamily>");
				builder.AppendLine(@"          <FontSize>12pt</FontSize>");
				builder.AppendLine(@"          <TextAlign>Right</TextAlign>");
				builder.AppendLine(@"        </Style>");
				builder.AppendLine(@"      </Textbox>");
			}

			builder.AppendLine(@"    </ReportItems>");
			builder.AppendLine(@"  </Body>");

			if(hasSignature)
			{
				builder.AppendLine(@"  <EmbeddedImages>");
				builder.AppendLine(@"    <EmbeddedImage Name=""SignatureImg"">");
				builder.AppendLine(@"      <MIMEType>image/png</MIMEType>");
				builder.AppendLine($"      <ImageData>{signatureBase64}</ImageData>");
				builder.AppendLine(@"    </EmbeddedImage>");
				builder.AppendLine(@"  </EmbeddedImages>");
			}

			builder.AppendLine(@"</Report>");
			return builder.ToString();
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

			// Неразрывные пробелы — стабильный визуальный отступ в RDL Textbox.
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

		private static string Escape(string value) =>
			SecurityElement.Escape(value ?? string.Empty) ?? string.Empty;
	}
}
