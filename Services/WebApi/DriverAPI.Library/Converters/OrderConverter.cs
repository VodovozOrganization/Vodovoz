using DriverAPI.Library.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;

namespace DriverAPI.Library.Converters
{
	public class OrderConverter
	{
		private readonly ILogger<OrderConverter> _logger;
		private readonly DeliveryPointConverter _deliveryPointConverter;
		private readonly SmsPaymentStatusConverter _smsPaymentConverter;
		private readonly PaymentTypeConverter _paymentTypeConverter;
		private readonly SignatureTypeConverter _signatureTypeConverter;

		public OrderConverter(ILogger<OrderConverter> logger,
			DeliveryPointConverter deliveryPointConverter,
			SmsPaymentStatusConverter smsPaymentConverter,
			PaymentTypeConverter paymentTypeConverter,
			SignatureTypeConverter signatureTypeConverter)
		{
			this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this._deliveryPointConverter = deliveryPointConverter ?? throw new ArgumentNullException(nameof(deliveryPointConverter));
			this._smsPaymentConverter = smsPaymentConverter ?? throw new ArgumentNullException(nameof(smsPaymentConverter));
			this._paymentTypeConverter = paymentTypeConverter ?? throw new ArgumentNullException(nameof(paymentTypeConverter));
			_signatureTypeConverter = signatureTypeConverter ?? throw new ArgumentNullException(nameof(signatureTypeConverter));
		}

		public OrderDto ConvertToApiOrder(Order vodovozOrder, SmsPaymentStatus? smsPaymentStatus, DateTime addedToRouteListTime)
		{
			var pairOfSplitedLists = SplitDeliveryItems(vodovozOrder.OrderEquipments);

			var deliveryPointPhones = vodovozOrder.DeliveryPoint.Phones
				.Select(x => new PhoneDto {Number = "+7" + x.DigitsNumber, PhoneType = PhoneDtoType.DeliveryPoint})
				.ToList();

			var counterpartyPhones = vodovozOrder.Client.Phones
				.Select(x => new PhoneDto { Number = "+7" + x.DigitsNumber, PhoneType = PhoneDtoType.Counterparty })
				.ToList();

			var apiOrder = new OrderDto
			{
				OrderId = vodovozOrder.Id,
				SmsPaymentStatus = _smsPaymentConverter.convertToAPIPaymentStatus(smsPaymentStatus),
				DeliveryTime = vodovozOrder.TimeDelivered?.ToString("HH:mm:ss"),
				FullBottleCount = vodovozOrder.Total19LBottlesToDeliver,
				EmptyBottlesToReturn = vodovozOrder.BottlesReturn ?? 0,
				Counterparty = vodovozOrder.Client.FullName,
				PhoneNumbers = deliveryPointPhones.Concat(counterpartyPhones),
				PaymentType = _paymentTypeConverter.ConvertToAPIPaymentType(vodovozOrder.PaymentType, vodovozOrder.PaymentByCardFrom),
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

		private (IEnumerable<OrderDeliveryItemDto> orderDeliveryItems, IEnumerable<OrderReceptionItemDto> orderReceptionItems)
			SplitDeliveryItems(IEnumerable<Vodovoz.Domain.Orders.OrderEquipment> orderEquipment)
		{
			var deliveryItems = new List<OrderDeliveryItemDto>();
			var receptionItems = new List<OrderReceptionItemDto>();

			foreach (var transferItem in orderEquipment)
			{
				if (transferItem.Direction == Vodovoz.Domain.Orders.Direction.Deliver)
				{
					deliveryItems.Add(ConvertToAPIOrderDeliveryItem(transferItem));
				}
				else if (transferItem.Direction == Vodovoz.Domain.Orders.Direction.PickUp)
				{
					receptionItems.Add(ConvertToAPIOrderReceptionItem(transferItem));
				}
			}

			return (deliveryItems, receptionItems);
		}

		private IEnumerable<OrderSaleItemDto> PrepareSaleItemsList(IEnumerable<Vodovoz.Domain.Orders.OrderItem> orderItems)
		{
			var result = new List<OrderSaleItemDto>();

			foreach (var saleItem in orderItems)
			{
				result.Add(ConvertToAPIOrderSaleItem(saleItem));
			}

			return result;
		}

		private OrderSaleItemDto ConvertToAPIOrderSaleItem(Vodovoz.Domain.Orders.OrderItem saleItem)
		{
			var result = new OrderSaleItemDto()
			{
				OrderSaleItemId = saleItem.Id,
				Name = saleItem.Nomenclature.Name,
				Quantity = saleItem.ActualCount ?? saleItem.Count,
				TotalOrderItemPrice = saleItem.ActualSum
			};

			return result;
		}

		private OrderDeliveryItemDto ConvertToAPIOrderDeliveryItem(Vodovoz.Domain.Orders.OrderEquipment saleItem)
		{
			var result = new OrderDeliveryItemDto()
			{
				OrderDeliveryItemId = saleItem.Id,
				Name = saleItem.Nomenclature.Name,
				Quantity = saleItem.ActualCount ?? saleItem.Count
			};

			return result;
		}

		private OrderReceptionItemDto ConvertToAPIOrderReceptionItem(Vodovoz.Domain.Orders.OrderEquipment saleItem)
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
