using System;
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
		public FiscalDocument CreateDocument(CashReceipt cashReceipt)
		{
			var order = cashReceipt.Order;
			var cashier = order.Contract?.Organization?.ActiveOrganizationVersion?.Leader?.ShortName;
			var documentId = string.Concat("vod_", order.Id);
			var time = (order.TimeDelivered ?? DateTime.Now).ToString("O");
			var contact = order.GetContact();

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

			FillInventPositions(fiscalDocument, cashReceipt);
			FillMoneyPositions(fiscalDocument, cashReceipt);

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
					throw new TrueMarkException("У одного из кодов не заполнен итоговый код который должен быть использован для записи в чек. " +
						"Возможно он оказался не обработанным службной обработки кодов честного знака");
				}

				if(orderItemsCodes.Count < orderItem.Count)
				{
					throw new TrueMarkException($"Невозможно сформировать строку в чеке. У номенклатуры Id {orderItem.Nomenclature.Id} " +
						$"включена обязательная маркировка, но для строки заказа Id {orderItem.Id} количество кодов ({orderItemsCodes.Count}) не " +
						$"совпадает с количеством товара ({orderItem.Count})");
				}

				if(orderItem.Count == 1)
				{
					var inventPosition = CreateInventPosition(orderItem);
					inventPosition.ProductMark = orderItemsCodes.First().ResultCode.RawCode;
					fiscalDocument.InventPositions.Add(inventPosition);
					continue;
				}

				decimal wholeDiscount = 0;

				//i == 1 чтобы пропуcтить последний элемент, у него расчет происходит из остатков
				for(int i = 1; i <= orderItem.Count - 1; i++)
				{
					decimal partDiscount = Math.Floor(orderItem.DiscountMoney / orderItem.Count);
					wholeDiscount += partDiscount;

					var inventPosition = CreateInventPosition(orderItem);
					inventPosition.Quantity = 1;
					inventPosition.DiscSum = partDiscount;
					inventPosition.ProductMark = orderItemsCodes[i - 1].ResultCode.RawCode;
					fiscalDocument.InventPositions.Add(inventPosition);
				}

				//добавление последнего элемента с остатками от целой скидки
				var orderItemCode = orderItemsCodes[(int)orderItem.Count - 1];

				var residueDiscount = orderItem.DiscountMoney - wholeDiscount;
				var lastInventPosition = CreateInventPosition(orderItem);
				lastInventPosition.Quantity = 1;
				lastInventPosition.DiscSum = residueDiscount;
				lastInventPosition.ProductMark = orderItemCode.ResultCode.RawCode;
				fiscalDocument.InventPositions.Add(lastInventPosition);
			}
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

			var moneyPosition = new MoneyPosition
			{
				Sum = sum,
				PaymentType = GetPaymentType(order)
			};

			fiscalDocument.MoneyPositions.Add(moneyPosition);
		}

		private string GetPaymentType(Order order)
		{
			switch(order.PaymentType)
			{
				case PaymentType.Terminal:
				case PaymentType.ByCard:
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
