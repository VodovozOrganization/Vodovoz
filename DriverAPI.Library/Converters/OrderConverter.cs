using DriverAPI.Library.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Services;

namespace DriverAPI.Library.Converters
{
    public class OrderConverter
    {
        private readonly ILogger<OrderConverter> logger;
        private readonly DeliveryPointConverter deliveryPointConverter;
        private readonly IOrderParametersProvider orderParametersProvider;
        private readonly SmsPaymentConverter smsPaymentConverter;

        public OrderConverter(ILogger<OrderConverter> logger,
            DeliveryPointConverter deliveryPointConverter,
            IOrderParametersProvider orderParametersProvider, 
            SmsPaymentConverter smsPaymentConverter)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.deliveryPointConverter = deliveryPointConverter ?? throw new ArgumentNullException(nameof(deliveryPointConverter));
            this.orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
            this.smsPaymentConverter = smsPaymentConverter ?? throw new ArgumentNullException(nameof(smsPaymentConverter));
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
                PaymentType = convertToAPIPaymentType(vodovozOrder.PaymentType, vodovozOrder.PaymentByCardFrom),
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

        private APIPaymentType convertToAPIPaymentType(PaymentType paymentType, Vodovoz.Domain.Orders.PaymentFrom paymentByCardFrom)
        {
            switch (paymentType)
            {
                case PaymentType.cash:
                    return APIPaymentType.Cash;
                case PaymentType.cashless:
                    return APIPaymentType.Cashless;
                case PaymentType.ByCard:
                    if (paymentByCardFrom.Id == orderParametersProvider.PaymentByCardFromSmsId)
                    {
                        return APIPaymentType.Sms;
                    }
                    else
                    {
                        return APIPaymentType.ByCard;
                    }
                case PaymentType.Terminal:
                    return APIPaymentType.Terminal;
                case PaymentType.BeveragesWorld:
                case PaymentType.barter:
                case PaymentType.ContractDoc:
                    return APIPaymentType.Payed;
                default:
                    logger.LogWarning($"Не поддерживается тип: {paymentType}");
                    throw new ArgumentException($"Не поддерживается тип: {paymentType}");
            }
        }
    }
}
