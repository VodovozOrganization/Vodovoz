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
    public class APISmsPaymentData : IAPISmsPaymentData
    {
        private readonly ILogger<APISmsPaymentData> logger;
        private readonly IOrderRepository orderRepository;
        private readonly IUnitOfWork unitOfWork;

        public APISmsPaymentData(ILogger<APISmsPaymentData> logger,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public SmsPaymentStatus? GetOrderPaymentStatus(int orderId)
        {
            return orderRepository.GetOrderPaymentStatus(unitOfWork, orderId);
        }
    }
}
