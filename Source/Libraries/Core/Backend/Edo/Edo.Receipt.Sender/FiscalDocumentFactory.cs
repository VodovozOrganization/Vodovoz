using Core.Infrastructure;
using ModulKassa.DTO;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Settings.Edo;

namespace Edo.Receipt.Sender
{
	public class FiscalDocumentFactory
	{
		private readonly IEdoReceiptSettings _edoReceiptSettings;

		public FiscalDocumentFactory(IEdoReceiptSettings edoReceiptSettings)
		{
			_edoReceiptSettings = edoReceiptSettings ?? throw new ArgumentNullException(nameof(edoReceiptSettings));
		}

		public FiscalDocument CreateFiscalDocument(EdoFiscalDocument edoFiscalDocument)
		{
			var document = new FiscalDocument
			{
				Id = GetDocumentGuid(edoFiscalDocument),
				DocNum = edoFiscalDocument.DocumentNumber,
				DocType = GetDocType(edoFiscalDocument.DocumentType),
				CheckoutDateTime = FormatCashboxDateTime(edoFiscalDocument.CheckoutTime),
				Email = edoFiscalDocument.Contact,
				ClientINN = NormalizeClientInn(edoFiscalDocument.ClientInn),
				CashierName = edoFiscalDocument.CashierName,
				PrintReceipt = edoFiscalDocument.PrintReceipt,
				ResponseURL = GetResponseUrl(edoFiscalDocument),

				//TaxMode null, потому что эта настройка применяется на уровне кассового аппарата
				TaxMode = null,
			};

			var inventPositions = edoFiscalDocument.InventPositions.Select(CreateInventPosition);
			document.InventPositions.AddRange(inventPositions);

			var moneyPositions = edoFiscalDocument.MoneyPositions.Select(CreateMoneyPosition);
			document.MoneyPositions.AddRange(moneyPositions);

			return document;
		}

		private static readonly TimeZoneInfo _moscowTimeZone = GetMoscowTimeZone();

		private static TimeZoneInfo GetMoscowTimeZone()
		{
			try
			{
				return TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
			}
			catch(TimeZoneNotFoundException)
			{
				try
				{
					return TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
				}
				catch
				{
					return TimeZoneInfo.CreateCustomTimeZone("Europe/Moscow", TimeSpan.FromHours(3), "MSK", "MSK");
				}
			}
		}

		private static string FormatCashboxDateTime(DateTime value)
		{
			DateTime moscowTime;
			if(value.Kind == DateTimeKind.Utc)
			{
				moscowTime = TimeZoneInfo.ConvertTimeFromUtc(value, _moscowTimeZone);
			}
			else if(value.Kind == DateTimeKind.Local)
			{
				moscowTime = TimeZoneInfo.ConvertTime(value, _moscowTimeZone);
			}
			else
			{
				// Unspecified — в БД Водовоза даты уже хранятся по московскому времени
				moscowTime = value;
			}

			// Передаем дату со смещением +00:00.
			// С одной стороны, наличие смещения строго требуется парсером API v2 МодульКассы (OffsetDateTime),
			// предотвращая 500 Internal Server Error.
			// С другой стороны, смещение +00:00 предотвращает вычитание 3 часов сервером кассы,
			// благодаря чему в личном кабинете отображается реальное исходное время (20:40:40, а не 17:40:40).
			return moscowTime.ToString("yyyy-MM-dd'T'HH:mm:ss+00:00");
		}

		private static string NormalizeClientInn(string clientInn)
		{
			if(string.IsNullOrWhiteSpace(clientInn))
			{
				return null;
			}

			return clientInn.Trim();
		}

		private string GetDocType(FiscalDocumentType fiscalDocumentType)
		{
			switch(fiscalDocumentType)
			{
				case FiscalDocumentType.Sale:
					return "SALE";
				case FiscalDocumentType.Return:
					return "RETURN";
				case FiscalDocumentType.Buy:
					return "BUY";
				case FiscalDocumentType.BuyReturn:
					return "BUY_RETURN";
				case FiscalDocumentType.SaleCorrection:
					return "SALE_CORRECTION";
				case FiscalDocumentType.SaleReturnCorrection:
					return "SALE_RETURN_CORRECTION";
				default:
					throw new InvalidOperationException($"Неизвестный тип фискального документа {fiscalDocumentType}");
			}
		}

		private string GetResponseUrl(EdoFiscalDocument edoFiscalDocument)
		{
			var documentGuid = GetDocumentGuid(edoFiscalDocument);
			var completeReceiptUrl = $"callback/complete/{documentGuid}";
			var uriBuilder = new UriBuilder(_edoReceiptSettings.EdoReceiptApiUrl)
			{
				Path = completeReceiptUrl
			};

			return uriBuilder.Uri.ToString();
		}

		private string GetDocumentGuid(EdoFiscalDocument edoFiscalDocument)
		{
			return edoFiscalDocument.DocumentGuid.ToString();
		}

		private static readonly Regex _timeParamRegex = new Regex(@"(?:^|&)Time=(\d+)", RegexOptions.Compiled);

		public static string NormalizeDocData(string docData)
		{
			if(string.IsNullOrEmpty(docData))
			{
				return docData;
			}

			return _timeParamRegex.Replace(docData, m =>
			{
				var digits = m.Groups[1].Value;
				if(long.TryParse(digits, out var timestamp) && timestamp > 100_000_000_000L)
				{
					var seconds = timestamp / 1000;
					return m.Value.Replace(digits, seconds.ToString());
				}
				return m.Value;
			});
		}

		private InventPosition CreateInventPosition(FiscalInventPosition fiscalInventPosition)
		{
			var inventPosition = new InventPosition
			{
				Name = fiscalInventPosition.Name,
				Quantity = fiscalInventPosition.Quantity,
				PriceWithoutDiscount = fiscalInventPosition.Price,
				DiscSum = fiscalInventPosition.DiscountSum,
				VatTag = (int)fiscalInventPosition.Vat,
			};

			if(fiscalInventPosition.EdoTaskItem != null)
			{
				var productCode = fiscalInventPosition.EdoTaskItem.ProductCode;
				var markCode = productCode?.ResultCode ?? productCode?.SourceCode;
				if(markCode != null)
				{
					inventPosition.ProductMark = markCode.FormatForCheck1260;
				}
			}
			else if(fiscalInventPosition.GroupCode != null)
			{
				inventPosition.ProductMark = fiscalInventPosition.GroupCode.FormatForCheck1260;
			}

			if(!inventPosition.ProductMark.IsNullOrWhiteSpace() && fiscalInventPosition.RegulatoryDocument != null)
			{
				inventPosition.IndustryRequisite = new IndustryRequisite
				{
					FoivId = fiscalInventPosition.RegulatoryDocument.FoivId,
					DocNumber = fiscalInventPosition.RegulatoryDocument.DocNumber,
					DocDateTime = fiscalInventPosition.RegulatoryDocument.DocDateTime,
					DocData = NormalizeDocData(fiscalInventPosition.IndustryRequisiteData)
				};
			}

			return inventPosition;
		}

		private MoneyPosition CreateMoneyPosition(FiscalMoneyPosition fiscalMoneyPosition)
		{
			var moneyPosition = new MoneyPosition
			{
				PaymentType = GetPaymentType(fiscalMoneyPosition.PaymentType),
				Sum = fiscalMoneyPosition.Sum
			};
			return moneyPosition;
		}

		private string GetPaymentType(FiscalPaymentType fiscalPaymentType)
		{
			switch(fiscalPaymentType)
			{
				case FiscalPaymentType.Card:
					return "CARD";
				case FiscalPaymentType.Cash:
					return "CASH";
				case FiscalPaymentType.Prepaid:
					return "PREPAID";
				case FiscalPaymentType.Postpay:
					return "POSTPAY";
				case FiscalPaymentType.Other:
					return "OTHER";
				default:
					throw new InvalidOperationException($"Неизвестный тип оплаты {fiscalPaymentType}");
			}
		}
	}
}
