using DriverAPI.Library.DTOs;
using QS.Utilities.Numeric;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Orders;

namespace DriverAPI.Library.Converters
{
	public class OrderConverter
	{
		private readonly DeliveryPointConverter _deliveryPointConverter;
		private readonly SmsPaymentStatusConverter _smsPaymentConverter;
		private readonly PaymentTypeConverter _paymentTypeConverter;
		private readonly SignatureTypeConverter _signatureTypeConverter;
		private readonly QRPaymentConverter _qrPaymentConverter;

		public OrderConverter(
			DeliveryPointConverter deliveryPointConverter,
			SmsPaymentStatusConverter smsPaymentConverter,
			PaymentTypeConverter paymentTypeConverter,
			SignatureTypeConverter signatureTypeConverter,
			QRPaymentConverter qrPaymentConverter)
		{
			_deliveryPointConverter = deliveryPointConverter ?? throw new ArgumentNullException(nameof(deliveryPointConverter));
			_smsPaymentConverter = smsPaymentConverter ?? throw new ArgumentNullException(nameof(smsPaymentConverter));
			_paymentTypeConverter = paymentTypeConverter ?? throw new ArgumentNullException(nameof(paymentTypeConverter));
			_signatureTypeConverter = signatureTypeConverter ?? throw new ArgumentNullException(nameof(signatureTypeConverter));
			_qrPaymentConverter = qrPaymentConverter ?? throw new ArgumentNullException(nameof(qrPaymentConverter));
		}

		public OrderDto ConvertToAPIOrder(
			Order vodovozOrder,
			DateTime addedToRouteListTime,
			SmsPaymentStatus? smsPaymentStatus,
			FastPaymentStatus? qrPaymentDtoStatus)
		{
			var pairOfSplitedLists = SplitDeliveryItems(vodovozOrder.OrderEquipments);

			var apiOrder = new OrderDto
			{
				OrderId = vodovozOrder.Id,
				SmsPaymentStatus = _smsPaymentConverter.convertToAPIPaymentStatus(smsPaymentStatus),
				QRPaymentStatus = _qrPaymentConverter.ConvertToAPIPaymentStatus(qrPaymentDtoStatus),
				DeliveryTime = vodovozOrder.TimeDelivered?.ToString("HH:mm:ss"),
				FullBottleCount = vodovozOrder.Total19LBottlesToDeliver,
				EmptyBottlesToReturn = (vodovozOrder.BottlesReturn ?? 0) + vodovozOrder.BottlesByStockCount,
				Counterparty = vodovozOrder.Client.FullName,
				PhoneNumbers = CreatePhoneList(vodovozOrder),
				PaymentType = _paymentTypeConverter.ConvertToAPIPaymentType(vodovozOrder.PaymentType, qrPaymentDtoStatus == FastPaymentStatus.Performed, vodovozOrder.PaymentByTerminalSource),
				Address = _deliveryPointConverter.ExtractAPIAddressFromDeliveryPoint(vodovozOrder.DeliveryPoint),
				OrderComment = vodovozOrder.Comment,
				OrderSum = vodovozOrder.OrderSum,
				OrderSaleItems = PrepareSaleItemsList(vodovozOrder.OrderItems),
				OrderDeliveryItems = pairOfSplitedLists.orderDeliveryItems,
				OrderReceptionItems = pairOfSplitedLists.orderReceptionItems,
				IsFastDelivery = vodovozOrder.IsFastDelivery,
				AddedToRouteListTime = addedToRouteListTime.ToString("dd.MM.yyyyTHH:mm:ss"),
				Trifle = vodovozOrder.Trifle ?? 0,
				SignatureType = _signatureTypeConverter.ConvertToApiSignatureType(vodovozOrder.SignatureType)
			};

			return apiOrder;
		}

		private IEnumerable<PhoneDto> CreatePhoneList(Order vodovozOrder)
		{
			var phoneFormatter = new PhoneFormatter(PhoneFormat.RussiaOnlyShort);

			var deliveryPointPhones = vodovozOrder.DeliveryPoint.Phones
				.Where(p => !p.IsArchive)
				.GroupBy(p => p.DigitsNumber)
				.Select(x => new PhoneDto 
				{ 
					Number = phoneFormatter.FormatString(x.First().DigitsNumber),
					PhoneType = PhoneDtoType.DeliveryPoint 
				})
				.ToList();

			var counterpartyPhones = vodovozOrder.Client.Phones
				.Where(p => !p.IsArchive)
				.GroupBy(p => p.DigitsNumber)
				.Select(x => new PhoneDto 
				{ 
					Number = phoneFormatter.FormatString(x.First().DigitsNumber),
					PhoneType = PhoneDtoType.Counterparty 
				})
				.ToList();

			var allPhones = deliveryPointPhones.Concat(counterpartyPhones);

			var orderContactPhone = vodovozOrder.ContactPhone;

			if(orderContactPhone == null)
			{
				return allPhones;
			}

			var foundContactPhone = allPhones.FirstOrDefault(p => p.Number == phoneFormatter.FormatString(orderContactPhone.DigitsNumber));

			if(foundContactPhone == null)
			{
				return allPhones;
			}

			var resultPhoneList = new List<PhoneDto>
			{
				foundContactPhone
			};

			foreach(var phone in allPhones)
			{
				if(!resultPhoneList.Contains(phone))
				{
					resultPhoneList.Add(phone);
				}
			}

			return resultPhoneList;
		}

		private (IEnumerable<OrderDeliveryItemDto> orderDeliveryItems, IEnumerable<OrderReceptionItemDto> orderReceptionItems)
			SplitDeliveryItems(IEnumerable<OrderEquipment> orderEquipment)
		{
			var deliveryItems = new List<OrderDeliveryItemDto>();
			var receptionItems = new List<OrderReceptionItemDto>();

			foreach (var transferItem in orderEquipment)
			{
				if (transferItem.Direction == Direction.Deliver)
				{
					deliveryItems.Add(ConvertToAPIOrderDeliveryItem(transferItem));
				}
				else if (transferItem.Direction == Direction.PickUp)
				{
					receptionItems.Add(ConvertToAPIOrderReceptionItem(transferItem));
				}
			}

			return (deliveryItems, receptionItems);
		}

		private IEnumerable<OrderSaleItemDto> PrepareSaleItemsList(IEnumerable<OrderItem> orderItems)
		{
			var result = new List<OrderSaleItemDto>();

			foreach (var saleItem in orderItems)
			{
				result.Add(ConvertToAPIOrderSaleItem(saleItem));
			}

			return result;
		}

		private OrderSaleItemDto ConvertToAPIOrderSaleItem(OrderItem saleItem)
		{
			var result = new OrderSaleItemDto()
			{
				OrderSaleItemId = saleItem.Id,
				Name = saleItem.Nomenclature.Name,
				Quantity = saleItem.ActualCount ?? saleItem.Count,
				NeedScanCode = saleItem.Nomenclature.IsAccountableInTrueMark,
				OrderItemPrice = saleItem.Price,
				TotalOrderItemPrice = saleItem.ActualSum,
				IsBottleStock = saleItem.Order.IsBottleStock && saleItem.DiscountByStock > 0,
				IsDiscountInMoney = saleItem.IsDiscountInMoney,
				Discount = saleItem.IsDiscountInMoney ? saleItem.DiscountMoney : saleItem.Discount,
				DiscountReason = saleItem.DiscountReason?.Name
			};

			return result;
		}

		private OrderDeliveryItemDto ConvertToAPIOrderDeliveryItem(OrderEquipment saleItem)
		{
			var result = new OrderDeliveryItemDto()
			{
				OrderDeliveryItemId = saleItem.Id,
				Name = saleItem.Nomenclature.Name,
				Quantity = saleItem.ActualCount ?? saleItem.Count
			};

			return result;
		}

		private OrderReceptionItemDto ConvertToAPIOrderReceptionItem(OrderEquipment saleItem)
		{
			var result = new OrderReceptionItemDto()
			{
				OrderReceptionItemId = saleItem.Id,
				Name = saleItem.Nomenclature.Name,
				Quantity = saleItem.ActualCount ?? saleItem.Count
			};

			return result;
		}
	}
}
