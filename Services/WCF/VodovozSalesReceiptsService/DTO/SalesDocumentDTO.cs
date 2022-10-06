using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Vodovoz.Domain.Orders;

namespace VodovozSalesReceiptsService.DTO
{
	[DataContract]
	public class SalesDocumentDTO
	{
		public SalesDocumentDTO(Order order, string cashier)
		{
			CheckoutDateTime = (order.TimeDelivered ?? DateTime.Now).ToString("O");
			DocNum = Id = string.Concat("vod_", order.Id);
			Email = order.GetContact();
			CashierName = cashier;
			InventPositions = new List<InventPositionDTO>();
			foreach(var item in order.OrderItems) {
				InventPositions.Add(
					new InventPositionDTO {
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
	}
}