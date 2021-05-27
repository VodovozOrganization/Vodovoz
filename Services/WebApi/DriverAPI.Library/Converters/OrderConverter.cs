using DriverAPI.Library.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain;

namespace DriverAPI.Library.Converters
{
	public class OrderConverter
	{
		private readonly ILogger<OrderConverter> logger;
		private readonly DeliveryPointConverter deliveryPointConverter;
		private readonly SmsPaymentStatusConverter smsPaymentConverter;
		private readonly PaymentTypeConverter paymentTypeConverter;

		public OrderConverter(ILogger<OrderConverter> logger,
			DeliveryPointConverter deliveryPointConverter,
			SmsPaymentStatusConverter smsPaymentConverter,
			PaymentTypeConverter paymentTypeConverter)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.deliveryPointConverter = deliveryPointConverter ?? throw new ArgumentNullException(nameof(deliveryPointConverter));
			this.smsPaymentConverter = smsPaymentConverter ?? throw new ArgumentNullException(nameof(smsPaymentConverter));
			this.paymentTypeConverter = paymentTypeConverter ?? throw new ArgumentNullException(nameof(paymentTypeConverter));
		}

		public APIOrder convertToAPIOrder(Vodovoz.Domain.Orders.Order vodovozOrder, SmsPaymentStatus? smsPaymentStatus)
		{
			var pairOfSplitedLists = splitDeliveryItems(vodovozOrder.OrderEquipments);

			var apiOrder = new APIOrder()
			{
				OrderId = vodovozOrder.Id,
				SmsPaymentStatus = smsPaymentConverter.convertToAPIPaymentStatus(smsPaymentStatus),
				DeliveryTime = vodovozOrder.TimeDelivered?.ToString("HH:mm:ss"),
				FullBottleCount = vodovozOrder.Total19LBottlesToDeliver,
				Counterparty = vodovozOrder.Client.FullName,
				CounterpartyPhoneNumbers = vodovozOrder.Client.Phones.Select(x => "+7" + x.DigitsNumber),
				PaymentType = paymentTypeConverter.convertToAPIPaymentType(vodovozOrder.PaymentType, vodovozOrder.PaymentByCardFrom),
				Address = deliveryPointConverter.extractAPIAddressFromDeliveryPoint(vodovozOrder.DeliveryPoint),
				OrderComment = vodovozOrder.Comment,
				OrderSum = vodovozOrder.ActualTotalSum,
				OrderSaleItems = prepareSaleItemsList(vodovozOrder.OrderItems),
				OrderDeliveryItems = pairOfSplitedLists.orderDeliveryItems,
				OrderReceptionItems = pairOfSplitedLists.orderReceptionItems
			};

			return apiOrder;
		}

		private (IEnumerable<APIOrderDeliveryItem> orderDeliveryItems, IEnumerable<APIOrderReceptionItem> orderReceptionItems)
			splitDeliveryItems(IEnumerable<Vodovoz.Domain.Orders.OrderEquipment> orderEquipment)
		{
			var deliveryItems = new List<APIOrderDeliveryItem>();
			var receptionItems = new List<APIOrderReceptionItem>();

			foreach (var transferItem in orderEquipment)
			{
				if (transferItem.Direction == Vodovoz.Domain.Orders.Direction.Deliver)
				{
					deliveryItems.Add(convertToAPIOrderDeliveryItem(transferItem));
				}
				else if (transferItem.Direction == Vodovoz.Domain.Orders.Direction.PickUp)
				{
					receptionItems.Add(convertToAPIOrderReceptionItem(transferItem));
				}
			}

			return (deliveryItems, receptionItems);
		}

		private IEnumerable<APIOrderSaleItem> prepareSaleItemsList(IEnumerable<Vodovoz.Domain.Orders.OrderItem> orderItems)
		{
			var result = new List<APIOrderSaleItem>();

			foreach (var saleItem in orderItems)
			{
				result.Add(convertToAPIOrderSaleItem(saleItem));
			}

			return result;
		}

		private APIOrderSaleItem convertToAPIOrderSaleItem(Vodovoz.Domain.Orders.OrderItem saleItem)
		{
			var result = new APIOrderSaleItem()
			{
				OrderSaleItemId = saleItem.Id,
				Name = saleItem.Nomenclature.Name,
				Quantity = saleItem.ActualCount ?? saleItem.Count,
				TotalOrderItemPrice = saleItem.ActualSum
			};

			return result;
		}

		private APIOrderDeliveryItem convertToAPIOrderDeliveryItem(Vodovoz.Domain.Orders.OrderEquipment saleItem)
		{
			var result = new APIOrderDeliveryItem()
			{
				OrderDeliveryItemId = saleItem.Id,
				Name = saleItem.Nomenclature.Name,
				Quantity = saleItem.ActualCount ?? saleItem.Count
			};

			return result;
		}

		private APIOrderReceptionItem convertToAPIOrderReceptionItem(Vodovoz.Domain.Orders.OrderEquipment saleItem)
		{
			var result = new APIOrderReceptionItem()
			{
				OrderReceptionItemId = saleItem.Id,
				Name = saleItem.Nomenclature.Name,
				Quantity = saleItem.ActualCount ?? saleItem.Count
			};

			return result;
		}
	}
}
