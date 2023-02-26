using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;
using Vodovoz.Models.TrueMark;

namespace VodovozSalesReceiptsService.DTO
{
	[DataContract]
	public class SalesDocumentDTO
	{
		public SalesDocumentDTO(Order order, TrueMarkCashReceiptOrder trueMarkOrder, string cashier)
		{
			CheckoutDateTime = (order.TimeDelivered ?? DateTime.Now).ToString("O");
			DocNum = Id = string.Concat("vod_", order.Id);
			Email = order.GetContact();
			CashierName = cashier;
			InventPositions = new List<InventPositionDTO>();

			foreach(var orderItem in order.OrderItems) 
			{
				if(!orderItem.Nomenclature.IsAccountableInTrueMark)
				{
					var inventPosition = CreateInventPosition(orderItem);
					InventPositions.Add(inventPosition);
					continue;
				}

				var orderItemsCodes = trueMarkOrder.ScannedCodes
					.Where(x => x.OrderItem.Id == orderItem.Id)
					.ToList();

				if(orderItemsCodes.Any(x => string.IsNullOrWhiteSpace(x.ResultCode.RawCode)))
				{
					throw new TrueMarkException("У одного из кодов не заполнен итоговый код который должен быть использован для записи в чек. " +
						"Возможно он оказался не обработанным службной обработки кодов честного знака");
				}

				if(orderItemsCodes.Count != orderItem.Count)
				{
					throw new TrueMarkException($"Невозможно сформировать строку в чеке. У номенклатуры Id {orderItem.Nomenclature.Id} " +
						$"включена обязательная маркировка, но для строки заказа Id {orderItem.Id} количество кодов ({orderItemsCodes.Count}) не " +
						$"совпадает с количеством товара ({orderItem.Count})");
				}

				if(orderItem.Count == 1)
				{
					var inventPosition = CreateInventPosition(orderItem);
					inventPosition.ProductMark = orderItemsCodes.First().ResultCode.RawCode;
					InventPositions.Add(inventPosition);
					continue;
				}

				decimal wholeDiscount= 0;

				//i == 1 чтобы пропуcтить последний элемент, у него расчет происходит из остатков
				for(int i = 1; i <= orderItemsCodes.Count - 1; i++)
				{
					decimal partDiscount = Math.Floor(orderItem.DiscountMoney / orderItem.Count);
					wholeDiscount += partDiscount;

					var inventPosition = CreateInventPosition(orderItem);
					inventPosition.Quantity = 1;
					inventPosition.DiscSum = partDiscount;
					inventPosition.ProductMark = orderItemsCodes[i-1].ResultCode.RawCode;
					InventPositions.Add(inventPosition);
				}

				//добавление последнего элемента с остатками от целой скидки
				var orderItemCode = orderItemsCodes[orderItemsCodes.Count - 1];

				var residueDiscount = orderItem.DiscountMoney - wholeDiscount;
				var lastInventPosition = CreateInventPosition(orderItem);
				lastInventPosition.Quantity = 1;
				lastInventPosition.DiscSum = residueDiscount;
				lastInventPosition.ProductMark = orderItemCode.ResultCode.RawCode;
				InventPositions.Add(lastInventPosition);
			}

			MoneyPositions = new List<MoneyPositionDTO> {
				new MoneyPositionDTO(order, order.OrderItems.Sum(i => Math.Round(i.Price * i.Count - i.DiscountMoney, 2)))
			};
		}

		//Используется по старой логики для отправки юрикам с самовывозами
		//После запуска сканирования кодов на складе, необходимо будет отправлять с кодами
		public SalesDocumentDTO(Order order, string cashier)
		{
			CheckoutDateTime = (order.TimeDelivered ?? DateTime.Now).ToString("O");
			DocNum = Id = string.Concat("vod_", order.Id);
			Email = order.GetContact();
			CashierName = cashier;
			InventPositions = new List<InventPositionDTO>();
			foreach(var item in order.OrderItems)
			{
				InventPositions.Add(
					new InventPositionDTO
					{
						Name = item.Nomenclature.OfficialName,
						PriceWithoutDiscount = Math.Round(item.Price, 2),
						Quantity = item.Count,
						DiscSum = item.DiscountMoney,
						VatTag = (int)VatTag.VatFree
					}
				);
			}
			MoneyPositions = new List<MoneyPositionDTO> {
				new MoneyPositionDTO(order, order.OrderItems.Sum(i => Math.Round(i.Price * i.Count - i.DiscountMoney, 2)))
			};
		}

		[DataMember(IsRequired = true)]
		string id;
		public string Id {
			get => id;
			set => id = value;
		}

		[DataMember(IsRequired = true)]
		string docNum;
		public string DocNum {
			get => docNum;
			set => docNum = value;
		}

		#pragma warning disable CS0414
		
		[DataMember(IsRequired = true)]
		readonly string docType = "SALE";
		
		#pragma warning restore CS0414

		[DataMember(IsRequired = true)]
		string checkoutDateTime;			// Дата/время доставки заказа
		public string CheckoutDateTime {
			get => checkoutDateTime;
			set => checkoutDateTime = value;
		}

		[DataMember(IsRequired = true)]
		string email;
		public string Email {
			get => email;
			set => email = value;
		}

		[DataMember]
		bool printReceipt;
		public bool PrintReceipt {
			get => printReceipt;
			set => printReceipt = value;
		}

		[DataMember]
		string cashierName;
		public string CashierName {
			get => cashierName;
			set => cashierName = value;
		}

		[DataMember]
		string cashierPosition;
		public string CashierPosition {
			get => cashierPosition;
			set => cashierPosition = value;
		}

		[DataMember]
		string responseURL;
		public string ResponseURL {
			get => responseURL;
			set => responseURL = value;
		}

		[DataMember]
		string taxMode;
		public string TaxMode {
			get => taxMode;
			set => taxMode = value;
		}

		[DataMember(IsRequired = true)]
		List<InventPositionDTO> inventPositions;
		public List<InventPositionDTO> InventPositions {
			get => inventPositions;
			set => inventPositions = value;
		}

		[DataMember(IsRequired = true)]
		List<MoneyPositionDTO> moneyPositions;
		public List<MoneyPositionDTO> MoneyPositions {
			get => moneyPositions;
			set => moneyPositions = value;
		}

		public bool IsValid {
			get {
				return Id != null
					&& DocNum != null
					&& Email != null
					&& InventPositions != null
					&& MoneyPositions != null
					&& InventPositions.Any()
					&& MoneyPositions.Sum(x => x.Sum) > 0;
			}
		}

		private InventPositionDTO CreateInventPosition(OrderItem orderItem)
		{
			var inventPosition = new InventPositionDTO
			{
				Name = orderItem.Nomenclature.OfficialName,
				PriceWithoutDiscount = Math.Round(orderItem.Price, 2),
				Quantity = orderItem.Count,
				DiscSum = orderItem.DiscountMoney,
				VatTag = (int)VatTag.VatFree
			};
			return inventPosition;
		}
	}
}
