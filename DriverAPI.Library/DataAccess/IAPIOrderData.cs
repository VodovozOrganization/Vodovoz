using DriverAPI.Library.Models;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;

namespace DriverAPI.Library.DataAccess
{
    public interface IAPIOrderData
    {
        APIOrder Get(int orderId);
        IEnumerable<APIOrder> Get(int[] orderIds);
        APIOrderAdditionalInfo GetAdditionalInfoOrNull(int orderId);
    }
}