using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;
using Vodovoz.Models.CashReceipts.DTO;
using Vodovoz.Models.TrueMark;

namespace Vodovoz.Models.CashReceipts
{
	public class FiscalDocumentPreparer
	{
		private const string _notValidRawCode =
			"У одного из кодов не заполнен итоговый код, который должен быть использован для записи в чек. " +
			"Возможно он оказался не обработанным службой обработки кодов честного знака";
		private readonly TrueMarkWaterCodeParser _codeParser;

		public FiscalDocumentPreparer(TrueMarkWaterCodeParser codeParser)
		{
			_codeParser = codeParser ?? throw new ArgumentNullException(nameof(codeParser));
		}

		public FiscalDocument CreateDocument(CashReceipt cashReceipt)
		{
			var order = cashReceipt.Order;
			var cashier = order.Contract?.Organization?.ActiveOrganizationVersion?.Leader?.ShortName;
			var documentId = cashReceipt.DocumentId;
			var time = (order.TimeDelivered ?? DateTime.Now).ToString("O");
			var contact = order.GetContact();
			cashReceipt.Contact = contact;

			var fiscalDocument = new FiscalDocument
			{
				Id = documentId,
				DocNum = documentId,
				CheckoutDateTime = time,
				Email = contact,
				CashierName = cashier
			};

			if(order.Client.ReasonForLeaving == ReasonForLeaving.Resale)
			{
				fiscalDocument.ClientINN = order.Client.INN;
			}

			var countMarkedNomenclatures =
				cashReceipt.Order.OrderItems.Where(x => x.Nomenclature.IsAccountableInTrueMark).Sum(x => x.Count);

			if(countMarkedNomenclatures > CashReceipt.MaxMarkCodesInReceipt)
			{
				FillInventPositions(fiscalDocument, cashReceipt, out var cashReceiptSum);
				FillMoneyPositions(fiscalDocument, cashReceipt.Order.PaymentType, cashReceiptSum);
			}
			else
			{
				FillInventPositions(fiscalDocument, cashReceipt);
				FillMoneyPositions(fiscalDocument, cashReceipt);
			}

			ValidateDocument(fiscalDocument);

			return fiscalDocument;
		}

		private void FillInventPositions(FiscalDocument fiscalDocument, CashReceipt cashReceipt)
		{
			foreach(var orderItem in cashReceipt.Order.OrderItems)
			{
				if(orderItem.Count <= 0)
				{
					continue;
				}

				if(!orderItem.Nomenclature.IsAccountableInTrueMark)
				{
					var inventPosition = CreateInventPosition(orderItem);
					fiscalDocument.InventPositions.Add(inventPosition);
					continue;
				}

				var orderItemsCodes = cashReceipt.ScannedCodes
					.Where(x => x.OrderItem.Id == orderItem.Id)
					.ToList();

				if(orderItemsCodes.Any(x => string.IsNullOrWhiteSpace(x.ResultCode.RawCode)))
				{
					throw new TrueMarkException(_notValidRawCode);
				}

				if(orderItemsCodes.Count < orderItem.Count)
				{
					throw new TrueMarkException(
						$"Невозможно сформировать строку в чеке. У номенклатуры Id {orderItem.Nomenclature.Id} " +
						$"включена обязательная маркировка, но для строки заказа Id {orderItem.Id} количество кодов ({orderItemsCodes.Count}) не " +
						$"совпадает с количеством товара ({orderItem.Count})");
				}

				if(orderItem.Count == 1)
				{
					var inventPosition = CreateInventPosition(orderItem);
					inventPosition.ProductMark = _codeParser.GetProductCodeForCashReceipt(orderItemsCodes.First().ResultCode);
					fiscalDocument.InventPositions.Add(inventPosition);
					continue;
				}

				decimal wholeDiscount = 0;

				//i == 1 чтобы пропуcтить последний элемент, у него расчет происходит из остатков
				for(int i = 1; i <= orderItem.Count - 1; i++)
				{
					var partDiscount = Math.Round(orderItem.DiscountMoney / orderItem.Count, 1);
					wholeDiscount += partDiscount;

					var inventPosition = CreateInventPosition(orderItem);
					inventPosition.Quantity = 1;
					inventPosition.DiscSum = partDiscount;
					inventPosition.ProductMark = _codeParser.GetProductCodeForCashReceipt(orderItemsCodes[i - 1].ResultCode);
					fiscalDocument.InventPositions.Add(inventPosition);
				}

				//добавление последнего элемента с остатками от целой скидки
				var orderItemCode = orderItemsCodes[(int)orderItem.Count - 1];

				var residueDiscount = orderItem.DiscountMoney - wholeDiscount;
				var lastInventPosition = CreateInventPosition(orderItem);
				lastInventPosition.Quantity = 1;
				lastInventPosition.DiscSum = residueDiscount;
				lastInventPosition.ProductMark = _codeParser.GetProductCodeForCashReceipt(orderItemCode.ResultCode);
				fiscalDocument.InventPositions.Add(lastInventPosition);
			}
		}

		private void FillInventPositions(FiscalDocument fiscalDocument, CashReceipt cashReceipt, out decimal cashReceiptSum)
		{
			cashReceiptSum = 0m;
			var maxCodesCount = CashReceipt.MaxMarkCodesInReceipt;
			var receiptNumber = cashReceipt.InnerNumber ?? 0;
			var unprocessedCodesCount = maxCodesCount * receiptNumber;

			if(unprocessedCodesCount == 0)
			{
				throw new InvalidOperationException(
					$"{nameof(cashReceipt.InnerNumber)} внутренний номер чека должен быть указан," +
					$" если маркированных позиций больше {maxCodesCount}");
			}
			
			foreach(var orderItem in cashReceipt.Order.OrderItems)
			{
				if(orderItem.Count <= 0)
				{
					continue;
				}

				if(!orderItem.Nomenclature.IsAccountableInTrueMark && receiptNumber == 1)
				{
					var inventPosition = CreateInventPosition(orderItem);
					fiscalDocument.InventPositions.Add(inventPosition);
					cashReceiptSum += orderItem.Sum;
					continue;
				}
				
				if(unprocessedCodesCount == 0)
				{
					continue;
				}

				if(!orderItem.Nomenclature.IsAccountableInTrueMark)
				{
					continue;
				}

				var orderItemsCodes =
					new Queue<CashReceiptProductCode>(cashReceipt.ScannedCodes.Where(x => x.OrderItem.Id == orderItem.Id));

				if(orderItemsCodes.Any(x => string.IsNullOrWhiteSpace(x.ResultCode.RawCode)))
				{
					throw new TrueMarkException(_notValidRawCode);
				}

				if(orderItem.Count == 1)
				{
					if(unprocessedCodesCount > maxCodesCount)
					{
						unprocessedCodesCount -= 1;
						continue;
					}
					
					var inventPosition = CreateInventPosition(orderItem);
					inventPosition.ProductMark =
						_codeParser.GetProductCodeForCashReceipt(TryGetCodeFromScannedCodes(orderItemsCodes, orderItem));
					fiscalDocument.InventPositions.Add(inventPosition);
					cashReceiptSum += orderItem.Sum;
					unprocessedCodesCount -= 1;
					continue;
				}

				var orderItemsCountWithoutLast = orderItem.Count - 1;
				var partDiscount = Math.Round(orderItem.DiscountMoney / orderItem.Count, 1);
				var lastPartDiscount = Math.Round(orderItem.DiscountMoney - (orderItemsCountWithoutLast * partDiscount), 2);

				for(var i = 0; i < orderItem.Count; i++)
				{
					if(unprocessedCodesCount > maxCodesCount)
					{
						unprocessedCodesCount -= 1;
						continue;
					}
					if(unprocessedCodesCount == 0)
					{
						break;
					}

					var discount = i == orderItemsCountWithoutLast ? lastPartDiscount : partDiscount;
					
					var inventPosition = CreateInventPosition(orderItem);
					inventPosition.Quantity = 1;
					inventPosition.DiscSum = discount;
					inventPosition.ProductMark =
						_codeParser.GetProductCodeForCashReceipt(TryGetCodeFromScannedCodes(orderItemsCodes, orderItem));
					fiscalDocument.InventPositions.Add(inventPosition);
					cashReceiptSum += inventPosition.PriceWithoutDiscount - discount;
					unprocessedCodesCount -= 1;
				}
			}
		}

		private TrueMarkWaterIdentificationCode TryGetCodeFromScannedCodes(
			Queue<CashReceiptProductCode> orderItemsCodes, OrderItem orderItem)
		{
			if(orderItemsCodes.Count == 0)
			{
				throw new TrueMarkException(
					$"Невозможно сформировать строку в чеке. У номенклатуры Id {orderItem.Nomenclature.Id} " +
					$"включена обязательная маркировка, но для строки заказа Id {orderItem.Id} количество кодов ({orderItemsCodes.Count}) не " +
					$"совпадает с количеством товара ({orderItem.Count})");
			}
			
			return orderItemsCodes.Dequeue().ResultCode;
		}

		private InventPosition CreateInventPosition(OrderItem orderItem)
		{
			var inventPosition = new InventPosition
			{
				Name = orderItem.Nomenclature.OfficialName,
				PriceWithoutDiscount = Math.Round(orderItem.Price, 2),
				Quantity = orderItem.Count,
				DiscSum = orderItem.DiscountMoney,
				VatTag = (int)VatTag.VatFree
			};
			return inventPosition;
		}

		private void FillMoneyPositions(FiscalDocument fiscalDocument, CashReceipt cashReceipt)
		{
			var order = cashReceipt.Order;
			var soldItems = order.OrderItems.Where(x => x.Count > 0);
			var sum = soldItems.Sum(x => x.Sum);

			AddMoneyPosition(fiscalDocument, order.PaymentType, sum);
		}
		
		private void FillMoneyPositions(FiscalDocument fiscalDocument, PaymentType orderPaymentType, decimal cashReceiptSum)
		{
			AddMoneyPosition(fiscalDocument, orderPaymentType, cashReceiptSum);
		}

		private void AddMoneyPosition(FiscalDocument fiscalDocument, PaymentType orderPaymentType, decimal cashReceiptSum)
		{
			var moneyPosition = new MoneyPosition
			{
				Sum = cashReceiptSum,
				PaymentType = GetPaymentType(orderPaymentType)
			};

			fiscalDocument.MoneyPositions.Add(moneyPosition);
		}

		private string GetPaymentType(PaymentType orderPaymentType)
		{
			switch(orderPaymentType)
			{
				case PaymentType.Terminal:
				case PaymentType.DriverApplicationQR:
				case PaymentType.SmsQR:
				case PaymentType.PaidOnline:
					return "CARD";
				default:
					return "CASH";
			}
		}

		private void ValidateDocument(FiscalDocument fiscalDocument)
		{
			if(fiscalDocument.Id == null)
			{
				throw new InvalidOperationException($"{nameof(fiscalDocument.Id)} фискального документа должен быть заполнен");
			}

			if(fiscalDocument.DocNum == null)
			{
				throw new InvalidOperationException($"{nameof(fiscalDocument.DocNum)} фискального документа должен быть заполнен");
			}

			if(fiscalDocument.Email == null)
			{
				throw new InvalidOperationException($"{nameof(fiscalDocument.Email)} фискального документа должен быть заполнен");
			}

			if(fiscalDocument.InventPositions == null || !fiscalDocument.InventPositions.Any())
			{
				throw new InvalidOperationException($"{nameof(fiscalDocument.InventPositions)} фискального документа должны быть заполнены");
			}

			if(fiscalDocument.MoneyPositions == null || !fiscalDocument.MoneyPositions.Any())
			{
				throw new InvalidOperationException($"{nameof(fiscalDocument.MoneyPositions)} фискального документа должны быть заполнены");
			}

			if(fiscalDocument.MoneyPositions != null && fiscalDocument.MoneyPositions.Sum(x => x.Sum) <= 0)
			{
				throw new InvalidOperationException($"Сумма в {nameof(fiscalDocument.MoneyPositions)} фискального документа должна быть больше нуля");
			}

			if(fiscalDocument.ClientINN.HasValue())
			{
				if(fiscalDocument.ClientINN.Length != 10 && fiscalDocument.ClientINN.Length != 12)
				{
					throw new InvalidOperationException($"ИНН контрагента ({fiscalDocument.ClientINN}) должен быть длиной 10 или 12 знаков");
				}

				if(fiscalDocument.ClientINN.Any(x => !char.IsNumber(x)))
				{
					throw new InvalidOperationException($"ИНН контрагента ({fiscalDocument.ClientINN}) должен состоять из цифр");
				}
			}
		}
	}
}
