using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Logistic
{
    public interface IWayBillDocumentRepository
    {
        IList<Order> GetOrdersForWayBillDocuments(IUnitOfWork uow, DateTime from, DateTime to);
    }
}