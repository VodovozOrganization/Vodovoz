using DriverAPI.Library.DTOs;
using Microsoft.Extensions.Logging;
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
		private readonly ILogger<OrderConverter> _logger;
		private readonly DeliveryPointConverter _deliveryPointConverter;
		private readonly SmsPaymentStatusConverter _smsPaymentConverter;
		private readonly PaymentTypeConverter _paymentTypeConverter;
		private readonly QRPaymentConverter _qrPaymentConverter;

		public OrderConverter(ILogger<OrderConverter> logger,
			DeliveryPointConverter deliveryPointConverter,
			SmsPaymentStatusConverter smsPaymentConverter,
			PaymentTypeConverter paymentTypeConverter,
			QRPaymentConverter qrPaymentConverter)
		{
			this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this._deliveryPointConverter = deliveryPointConverter ?? throw new ArgumentNullException(nameof(deliveryPointConverter));
			this._smsPaymentConverter = smsPaymentConverter ?? throw new ArgumentNullException(nameof(smsPaymentConverter));
			this._paymentTypeConverter = paymentTypeConverter ?? throw new ArgumentNullException(nameof(paymentTypeConverter));
			_qrPaymentConverter = qrPaymentConverter ?? throw new ArgumentNullException(nameof(qrPaymentConverter));
		}

		public OrderDto convertToAPIOrder(
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
				EmptyBottlesToReturn = vodovozOrder.BottlesReturn ?? 0,
				Counterparty = vodovozOrder.Client.FullName,
				PhoneNumbers = vodovozOrder.DeliveryPoint.Phones.Concat(vodovozOrder.Client.Phones).Select(x => "+7" + x.DigitsNumber),
				PaymentType = _paymentTypeConverter.ConvertToAPIPaymentType(vodovozOrder.PaymentType, vodovozOrder.PaymentByCardFrom),
				Address = _deliveryPointConverter.ExtractAPIAddressFromDeliveryPoint(vodovozOrder.DeliveryPoint),
				OrderComment = vodovozOrder.Comment,
				OrderSum = vodovozOrder.OrderSum,
				OrderSaleItems = PrepareSaleItemsList(vodovozOrder.OrderItems),
				OrderDeliveryItems = pairOfSplitedLists.orderDeliveryItems,
				OrderReceptionItems = pairOfSplitedLists.orderReceptionItems,
				IsFastDelivery = vodovozOrder.IsFastDelivery,
				AddedToRouteListTime = addedToRouteListTime.ToString("dd.MM.yyyyTHH:mm:ss")
			};

			return apiOrder;
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
				TotalOrderItemPrice = saleItem.ActualSum
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
