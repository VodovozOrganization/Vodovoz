using DriverAPI.Library.Models;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain;

namespace DriverAPI.Library.DataAccess
{
    public interface IAPISmsPaymentData
    {
        SmsPaymentStatus? GetOrderPaymentStatus(int orderId);
    }
}