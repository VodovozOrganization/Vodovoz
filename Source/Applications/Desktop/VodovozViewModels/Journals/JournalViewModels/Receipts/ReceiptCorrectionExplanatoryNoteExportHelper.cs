using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Vodovoz.ViewModels.Journals.JournalNodes.Receipts;
using Font = iTextSharp.text.Font;
using Image = iTextSharp.text.Image;
using Rectangle = iTextSharp.text.Rectangle;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Receipts
{
	public static class ReceiptCorrectionExplanatoryNoteExportHelper
	{
		private const string TitleLine = "Объяснительная записка";
		private const string FacsimilePlaceholder = "(факсимиле подписанта)";
		private static readonly Regex DateLineRegex = new Regex(@"^\d{2}\.\d{2}\.\d{4}$", RegexOptions.Compiled);

		public const string DocumentTitle = TitleLine;
		public const string SignaturePlaceholder = FacsimilePlaceholder;

		public static NoteLayout ParseNoteLayout(string content, DateTime createdDate)
		{
			var parsed = ParseNoteContent(content);
			return new NoteLayout
			{
				HeaderLines = parsed.HeaderLines,
				BodyLines = parsed.BodyLines,
				DateText = string.IsNullOrWhiteSpace(parsed.DateText)
					? createdDate.ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("ru-RU"))
					: parsed.DateText
			};
		}

		public static byte[] TryPrepareSignaturePng(byte[] signatureBytes)
		{
			if(signatureBytes == null || signatureBytes.Length == 0)
			{
				return null;
			}

			try
			{
				var cropped = CropSignatureToInk(signatureBytes);
				if(cropped != null && cropped.Length > 0)
				{
					return cropped;
				}

				using(var input = new MemoryStream(signatureBytes))
				using(var source = System.Drawing.Image.FromStream(input))
				using(var output = new MemoryStream())
				{
					source.Save(output, ImageFormat.Png);
					return output.ToArray();
				}
			}
			catch
			{
				return null;
			}
		}

		public sealed class NoteLayout
		{
			public List<string> HeaderLines { get; set; } = new List<string>();
			public List<string> BodyLines { get; set; } = new List<string>();
			public string DateText { get; set; }
		}

		public static void ExportRegistryToExcel(IList<ReceiptCorrectionExplanatoryNoteJournalNode> nodes, string path)
		{
			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Реестр");
				var headers = new[]
				{
					"№", "Дата формирования", "Организация", "Заказ", "Чек", "Основание"
				};

				for(var i = 0; i < headers.Length; i++)
				{
					worksheet.Cell(1, i + 1).Value = headers[i];
					worksheet.Cell(1, i + 1).Style.Font.Bold = true;
					worksheet.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				}

				var row = 2;
				foreach(var node in nodes)
				{
					worksheet.Cell(row, 1).Value = node.RowNumber;
					worksheet.Cell(row, 2).Value = node.CreatedDate;
					worksheet.Cell(row, 3).Value = node.OrganizationName;
					worksheet.Cell(row, 4).Value = node.OrderId;
					worksheet.Cell(row, 5).Value = node.FiscalDocumentNumber;
					worksheet.Cell(row, 6).Value = node.BasisTitle;
					row++;
				}

				worksheet.Columns().AdjustToContents();
				workbook.SaveAs(path);
			}
		}

		public static void ExportRegistryToPdf(IList<ReceiptCorrectionExplanatoryNoteJournalNode> nodes, string path)
		{
			using(var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
			using(var document = new Document(PageSize.A4.Rotate(), 20, 20, 20, 20))
			{
				PdfWriter.GetInstance(document, stream);
				document.Open();

				var font = CreateFont(9, Font.NORMAL);
				var bold = CreateFont(11, Font.BOLD);

				document.Add(new Paragraph($"Реестр пояснительных записок от {DateTime.Now:dd.MM.yyyy}", bold)
				{
					SpacingAfter = 12f
				});

				var table = new PdfPTable(6) { WidthPercentage = 100 };
				table.SetWidths(new float[] { 8, 16, 22, 12, 16, 26 });
				AddHeaderCell(table, "№", bold);
				AddHeaderCell(table, "Дата формирования", bold);
				AddHeaderCell(table, "Организация", bold);
				AddHeaderCell(table, "Заказ", bold);
				AddHeaderCell(table, "Чек", bold);
				AddHeaderCell(table, "Основание", bold);

				foreach(var node in nodes)
				{
					AddCell(table, node.RowNumber.ToString(), font);
					AddCell(table, node.CreatedDate.ToString("dd.MM.yyyy HH:mm"), font);
					AddCell(table, node.OrganizationName ?? string.Empty, font);
					AddCell(table, node.OrderId.ToString(), font);
					AddCell(table, node.FiscalDocumentNumber ?? string.Empty, font);
					AddCell(table, node.BasisTitle ?? string.Empty, font);
				}

				document.Add(table);
				document.Close();
			}
		}

		public static void ExportNotesToPdf(
			IList<ReceiptCorrectionExplanatoryNoteJournalNode> nodes,
			IDictionary<int, byte[]> signaturesById,
			string path)
		{
			using(var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
			using(var document = new Document(PageSize.A4, 50, 50, 50, 50))
			{
				PdfWriter.GetInstance(document, stream);
				document.Open();

				var font = CreateFont(12, Font.NORMAL);
				var titleFont = CreateFont(14, Font.BOLD);
				var first = true;

				foreach(var node in nodes)
				{
					if(!first)
					{
						document.NewPage();
					}

					first = false;
					RenderNote(document, node, signaturesById, font, titleFont);
				}

				document.Close();
			}
		}

		private static void RenderNote(
			Document document,
			ReceiptCorrectionExplanatoryNoteJournalNode node,
			IDictionary<int, byte[]> signaturesById,
			Font font,
			Font titleFont)
		{
			var parsed = ParseNoteContent(node.Content);

			foreach(var line in parsed.HeaderLines)
			{
				document.Add(new Paragraph(line, font)
				{
					Alignment = Element.ALIGN_RIGHT,
					SpacingAfter = 1f
				});
			}

			document.Add(new Paragraph(" ", font) { SpacingAfter = 8f });
			document.Add(new Paragraph(TitleLine, titleFont)
			{
				Alignment = Element.ALIGN_CENTER,
				SpacingAfter = 14f
			});

			foreach(var line in parsed.BodyLines)
			{
				var paragraph = new Paragraph(line.Length == 0 ? " " : line, font)
				{
					Alignment = Element.ALIGN_JUSTIFIED,
					SpacingAfter = 6f,
					FirstLineIndent = line.Length == 0 ? 0f : 20f
				};
				document.Add(paragraph);
			}

			document.Add(new Paragraph(" ", font) { SpacingBefore = 18f });

			Image signature = null;
			if(node.SignerSignatureId.HasValue
				&& signaturesById != null
				&& signaturesById.TryGetValue(node.SignerSignatureId.Value, out var bytes)
				&& bytes != null
				&& bytes.Length > 0)
			{
				try
				{
					var cropped = CropSignatureToInk(bytes) ?? bytes;
					signature = Image.GetInstance(cropped);
					signature.ScaleToFit(280f, 120f);
				}
				catch
				{
					signature = null;
				}
			}

			var footer = new PdfPTable(2)
			{
				WidthPercentage = 100,
				SpacingBefore = 8f
			};
			footer.SetWidths(new float[] { 1f, 1f });

			var dateText = string.IsNullOrWhiteSpace(parsed.DateText)
				? node.CreatedDate.ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("ru-RU"))
				: parsed.DateText;

			footer.AddCell(new PdfPCell(new Phrase(dateText, font))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_TOP,
				Padding = 0f
			});

			var rightCell = new PdfPCell
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_TOP,
				Padding = 0f
			};

			if(signature != null)
			{
				signature.Alignment = Element.ALIGN_RIGHT;
				rightCell.AddElement(signature);
			}
			else
			{
				rightCell.AddElement(new Paragraph(FacsimilePlaceholder, font)
				{
					Alignment = Element.ALIGN_RIGHT
				});
			}

			footer.AddCell(rightCell);
			document.Add(footer);
		}

		private static NoteContentParts ParseNoteContent(string content)
		{
			var result = new NoteContentParts();
			if(string.IsNullOrWhiteSpace(content))
			{
				result.BodyLines.Add("(текст отсутствует)");
				return result;
			}

			var lines = content.Replace("\r\n", "\n").Split('\n').ToList();
			var titleIndex = lines.FindIndex(x => string.Equals(x.Trim(), TitleLine, StringComparison.OrdinalIgnoreCase));

			if(titleIndex < 0)
			{
				result.BodyLines.AddRange(lines.Where(x => !IsFooterLine(x)));
				result.DateText = lines.LastOrDefault(IsDateLine)?.Trim();
				return result;
			}

			result.HeaderLines.AddRange(
				lines.Take(titleIndex)
					.Select(x => x.TrimEnd())
					.Where(x => !string.IsNullOrWhiteSpace(x)));

			var afterTitle = lines.Skip(titleIndex + 1).ToList();
			var footerStart = afterTitle.FindIndex(IsFooterLine);
			if(footerStart < 0)
			{
				result.BodyLines.AddRange(TrimBody(afterTitle));
				return result;
			}

			result.BodyLines.AddRange(TrimBody(afterTitle.Take(footerStart)));
			result.DateText = afterTitle.Skip(footerStart).FirstOrDefault(IsDateLine)?.Trim();
			return result;
		}

		private static IEnumerable<string> TrimBody(IEnumerable<string> lines)
		{
			var list = lines.Select(x => x.TrimEnd()).ToList();
			while(list.Count > 0 && string.IsNullOrWhiteSpace(list[0]))
			{
				list.RemoveAt(0);
			}

			while(list.Count > 0 && string.IsNullOrWhiteSpace(list[list.Count - 1]))
			{
				list.RemoveAt(list.Count - 1);
			}

			return list;
		}

		private static bool IsDateLine(string line) =>
			!string.IsNullOrWhiteSpace(line) && DateLineRegex.IsMatch(line.Trim());

		private static bool IsFooterLine(string line)
		{
			if(string.IsNullOrWhiteSpace(line))
			{
				return false;
			}

			var trimmed = line.Trim();
			return IsDateLine(trimmed)
				|| string.Equals(trimmed, FacsimilePlaceholder, StringComparison.OrdinalIgnoreCase);
		}

		private static byte[] CropSignatureToInk(byte[] bytes)
		{
			using(var input = new MemoryStream(bytes))
			using(var source = System.Drawing.Image.FromStream(input))
			using(var bitmap = new Bitmap(source))
			{
				var regionWidth = Math.Min(bitmap.Width, Math.Max(280, bitmap.Width / 2));
				var regionHeight = Math.Min(bitmap.Height, Math.Max(160, bitmap.Height / 6));
				var region = new System.Drawing.Rectangle(0, 0, regionWidth, regionHeight);

				var minX = region.Right;
				var minY = region.Bottom;
				var maxX = region.Left;
				var maxY = region.Top;
				var found = false;

				for(var y = region.Top; y < region.Bottom; y++)
				{
					for(var x = region.Left; x < region.Right; x++)
					{
						if(!IsInkPixel(bitmap.GetPixel(x, y)))
						{
							continue;
						}

						found = true;
						if(x < minX) minX = x;
						if(y < minY) minY = y;
						if(x > maxX) maxX = x;
						if(y > maxY) maxY = y;
					}
				}

				if(!found)
				{
					minX = region.Left;
					minY = region.Top;
					maxX = region.Right - 1;
					maxY = region.Bottom - 1;
				}

				var pad = 8;
				minX = Math.Max(0, minX - pad);
				minY = Math.Max(0, minY - pad);
				maxX = Math.Min(bitmap.Width - 1, maxX + pad);
				maxY = Math.Min(bitmap.Height - 1, maxY + pad);

				var width = maxX - minX + 1;
				var height = maxY - minY + 1;
				if(width < 20 || height < 10)
				{
					return null;
				}

				using(var cropped = bitmap.Clone(new System.Drawing.Rectangle(minX, minY, width, height), bitmap.PixelFormat))
				using(var output = new MemoryStream())
				{
					cropped.Save(output, ImageFormat.Png);
					return output.ToArray();
				}
			}
		}

		private static bool IsInkPixel(Color color)
		{
			if(color.A < 20)
			{
				return false;
			}

			// Синяя/тёмная подпись на белом фоне; отсекаем шум скана.
			var average = (color.R + color.G + color.B) / 3;
			return average < 170;
		}

		private static Font CreateFont(float size, int style)
		{
			var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
			var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
			return new Font(baseFont, size, style);
		}

		private static void AddHeaderCell(PdfPTable table, string text, Font font)
		{
			table.AddCell(new PdfPCell(new Phrase(text, font))
			{
				HorizontalAlignment = Element.ALIGN_CENTER,
				Padding = 4f,
				BackgroundColor = BaseColor.LIGHT_GRAY
			});
		}

		private static void AddCell(PdfPTable table, string text, Font font)
		{
			table.AddCell(new PdfPCell(new Phrase(text ?? string.Empty, font))
			{
				Padding = 3f
			});
		}

		private sealed class NoteContentParts
		{
			public List<string> HeaderLines { get; } = new List<string>();
			public List<string> BodyLines { get; } = new List<string>();
			public string DateText { get; set; }
		}
	}
}
