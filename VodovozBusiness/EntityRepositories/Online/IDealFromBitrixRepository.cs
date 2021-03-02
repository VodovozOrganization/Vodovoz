using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace Vodovoz.EntityRepositories {
    public interface IDealFromBitrixRepository
    {
        IList<DealFromBitrix> GetAllFailed(IUnitOfWork uow, DateTime startDate, DateTime endDate);
        DealFromBitrix GetByBitrixId(IUnitOfWork uow, uint bitrixId);
    }
}