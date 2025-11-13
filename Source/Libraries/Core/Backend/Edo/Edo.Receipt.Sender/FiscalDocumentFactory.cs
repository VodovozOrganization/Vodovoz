using Core.Infrastructure;
using ModulKassa.DTO;
using System;
using System.Linq;
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
				CheckoutDateTime = edoFiscalDocument.CheckoutTime.ToString("O"),
				Email = edoFiscalDocument.Contact,
				ClientINN = edoFiscalDocument.ClientInn,
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
				inventPosition.ProductMark = fiscalInventPosition.EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;
			}
			else if(fiscalInventPosition.GroupCode != null)
			{
				inventPosition.ProductMark = fiscalInventPosition.GroupCode.FormatForCheck1260;
			}

			if(!inventPosition.ProductMark.IsNullOrWhiteSpace())
			{
				inventPosition.IndustryRequisite = new IndustryRequisite
				{
					FoivId = fiscalInventPosition.RegulatoryDocument.FoivId,
					DocNumber = fiscalInventPosition.RegulatoryDocument.DocNumber,
					DocDateTime = fiscalInventPosition.RegulatoryDocument.DocDateTime,
					DocData = fiscalInventPosition.IndustryRequisiteData
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
