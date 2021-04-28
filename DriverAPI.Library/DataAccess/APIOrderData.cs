using DriverAPI.Library.Converters;
using DriverAPI.Library.Models;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.EntityRepositories.Orders;

namespace DriverAPI.Library.DataAccess
{
    public class APIOrderData : IAPIOrderData
    {
        private readonly ILogger<APIOrderData> logger;
        private readonly IOrderRepository orderRepository;
        private readonly OrderConverter orderConverter;
        private readonly IAPISmsPaymentData aPISmsPaymentData;
        private readonly IUnitOfWork unitOfWork;

        public APIOrderData(ILogger<APIOrderData> logger,
            IOrderRepository orderRepository,
            OrderConverter orderConverter,
            IAPISmsPaymentData aPISmsPaymentData,
            IUnitOfWork unitOfWork
            )
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            this.orderConverter = orderConverter ?? throw new ArgumentNullException(nameof(orderConverter));
            this.aPISmsPaymentData = aPISmsPaymentData ?? throw new ArgumentNullException(nameof(aPISmsPaymentData));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public APIOrder Get(int orderId) => Get(new int[] { orderId }).Single();

        public IEnumerable<APIOrder> Get(int[] orderIds)
        {
            var result = new List<APIOrder>();

            var vodovozOrders = orderRepository.GetOrders(unitOfWork, orderIds);

            foreach (var vodovozOrder in vodovozOrders)
            {
                try
                {
                    var smsPaymentStatus = aPISmsPaymentData.GetOrderPaymentStatus(vodovozOrder.Id);
                    var order = orderConverter.convertToAPIOrder(vodovozOrder, smsPaymentStatus);
                    order.OrderAdditionalInfo = GetAdditionalInfo(vodovozOrder);
                    result.Add(order);
                }
                catch (ArgumentException e)
                {
                    logger.LogWarning(e, $"Ошибка конвертирования заказа {vodovozOrder.Id}");
                }
            }

            return result;
        }

        public IEnumerable<APIPaymentType> GetAvailableToChangeStatuses(Vodovoz.Domain.Orders.Order order)
        {
            var availablePaymentTypes = new List<APIPaymentType>();

            if (order.PaymentType == Vodovoz.Domain.Client.PaymentType.cash)
            {
                availablePaymentTypes.Add(APIPaymentType.Terminal);
            }

            if (order.PaymentType == Vodovoz.Domain.Client.PaymentType.Terminal)
            {
                availablePaymentTypes.Add(APIPaymentType.Cash);
            }

            return availablePaymentTypes;
        }

        public APIOrderAdditionalInfo GetAdditionalInfoOrNull(int orderId) 
        {
            var vodovozOrder = orderRepository.GetOrders(unitOfWork, new[] { orderId }).FirstOrDefault();

            if (vodovozOrder == null)
            {
                return null;
            }

            return GetAdditionalInfo(vodovozOrder);
        }

        public APIOrderAdditionalInfo GetAdditionalInfo(Vodovoz.Domain.Orders.Order order)
        {
            return new APIOrderAdditionalInfo()
            {
                AvailablePaymentEnumTypes = GetAvailableToChangeStatuses(order),
                CanSendSms = CanSendSmsForPayment(order, aPISmsPaymentData.GetOrderPaymentStatus(order.Id)),
            };
        }

        private bool CanSendSmsForPayment(Vodovoz.Domain.Orders.Order order, SmsPaymentStatus? smsPaymentStatus)
        {
            return order.PaymentType == Vodovoz.Domain.Client.PaymentType.ByCard
                && smsPaymentStatus == Vodovoz.Domain.SmsPaymentStatus.Paid;
        }
    }
}
