using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.TrueMark;
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

			var countMarkedNomenclaturesWithPositiveSum =
				cashReceipt.Order.OrderItems
					.Where(x => x.Nomenclature.IsAccountableInTrueMark && x.Sum > 0)
					.Sum(x => x.Count);

			if(countMarkedNomenclaturesWithPositiveSum > CashReceipt.MaxMarkCodesInReceipt)
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
				if(orderItem.HasZeroCountOrSum())
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

				CheckCodes(orderItemsCodes);

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
					var code = orderItemsCodes.First().ResultCode;
					inventPosition.ProductMark = _codeParser.GetProductCodeForCashReceipt(code);
					CreateIndustryRequisite(inventPosition, code);

					fiscalDocument.InventPositions.Add(inventPosition);
					continue;
				}

				decimal wholeDiscount = 0;

				//i == 1 чтобы пропуcтить последний элемент, у него расчет происходит из остатков
				for(int i = 1; i <= orderItem.Count - 1; i++)
				{
					var inventPosition = CreateInventPosition(orderItem);

					if(wholeDiscount < orderItem.DiscountMoney)
					{
						var partDiscount = Math.Round(orderItem.DiscountMoney / orderItem.Count, 1);
						wholeDiscount += partDiscount;
						inventPosition.DiscSum = partDiscount;
					}
					else
					{
						inventPosition.DiscSum = 0;
					}

					inventPosition.Quantity = 1;
					var code = orderItemsCodes[i - 1].ResultCode;
					inventPosition.ProductMark = _codeParser.GetProductCodeForCashReceipt(code);
					CreateIndustryRequisite(inventPosition, code);
					fiscalDocument.InventPositions.Add(inventPosition);
				}

				//добавление последнего элемента с остатками от целой скидки
				var orderItemCode = orderItemsCodes[(int)orderItem.Count - 1];

				var residueDiscount = orderItem.DiscountMoney - wholeDiscount;
				if(residueDiscount < 0)
				{
					residueDiscount = 0;
				}

				var lastInventPosition = CreateInventPosition(orderItem);
				lastInventPosition.Quantity = 1;
				lastInventPosition.DiscSum = residueDiscount;
				var lastCode = orderItemCode.ResultCode;
				lastInventPosition.ProductMark = _codeParser.GetProductCodeForCashReceipt(lastCode);
				CreateIndustryRequisite(lastInventPosition, lastCode);
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
				if(orderItem.HasZeroCountOrSum())
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

				CheckCodes(orderItemsCodes);

				if(orderItem.Count == 1)
				{
					if(unprocessedCodesCount > maxCodesCount)
					{
						unprocessedCodesCount -= 1;
						continue;
					}

					var code = TryGetCodeFromScannedCodes(orderItemsCodes, orderItem);
					var inventPosition = CreateInventPosition(orderItem);
					inventPosition.ProductMark = _codeParser.GetProductCodeForCashReceipt(code);
					CreateIndustryRequisite(inventPosition, code);
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
					var code = TryGetCodeFromScannedCodes(orderItemsCodes, orderItem);
					inventPosition.ProductMark = _codeParser.GetProductCodeForCashReceipt(code);
					CreateIndustryRequisite(inventPosition, code);
					fiscalDocument.InventPositions.Add(inventPosition);
					cashReceiptSum += inventPosition.PriceWithoutDiscount - discount;
					unprocessedCodesCount -= 1;
				}
			}
		}

		private void CheckCodes(IEnumerable<CashReceiptProductCode> productCodes)
		{
			foreach(var productCode in productCodes)
			{
				if(string.IsNullOrWhiteSpace(productCode.ResultCode.RawCode))
				{
					throw new TrueMarkException(_notValidRawCode);
				}
				
				if(!productCode.ResultCode.IsTag1260Valid)
				{
					throw new TrueMarkException("В чеке содержатся коды, не прошедшие разрешительный режим(тэг 1260)");
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
				DiscSum = orderItem.DiscountMoney
			};

			SetVatProperties(orderItem, inventPosition);
			return inventPosition;
		}

		private void CreateIndustryRequisite(InventPosition inventPosition, TrueMarkWaterIdentificationCode code)
		{
			inventPosition.IndustryRequisite = new IndustryRequisite
			{
				DocData = $"UUID={code.Tag1260CodeCheckResult.ReqId}&Time={code.Tag1260CodeCheckResult.ReqTimestamp}"
			};
		}

		private void SetVatProperties(OrderItem orderItem, InventPosition inventPosition)
		{
			var organization = orderItem.Order.Contract?.Organization;

			if(organization != null)
			{
				var actualVatVersion = organization.IsUsnMode
					? organization.GetActualVatRateVersion(orderItem.Order.DeliveryDate)
					: orderItem.Nomenclature.GetActualVatRateVersion(orderItem.Order.DeliveryDate);

				if(actualVatVersion?.VatRate.VatRateValue == 0)
				{
					inventPosition.VatTag = (int)VatTag.VatFree;
					return;
				}
			}
			else if(orderItem.Nomenclature.GetActualVatRateVersion(orderItem.Order.DeliveryDate)?.VatRate.VatRateValue == 0)
			{
				inventPosition.VatTag = (int)VatTag.VatFree;
				return;
			}

			inventPosition.VatTag = (int)VatTag.Vat20;
		}

		private void FillMoneyPositions(FiscalDocument fiscalDocument, CashReceipt cashReceipt)
		{
			var order = cashReceipt.Order;
			var soldItems = order.OrderItems.Where(x => x.Count > 0 && x.Sum > 0);
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
