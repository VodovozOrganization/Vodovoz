using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace Vodovoz.EntityRepositories {
    public class DealFromBitrixRepository: IDealFromBitrixRepository {
        public IList<DealFromBitrix> GetAllFailed(IUnitOfWork uow, DateTime startDate, DateTime endDate)
        {
            return uow.Session.QueryOver<DealFromBitrix>()
                .Where(dp => dp.Success == false)
                .Where(dp => dp.CreateDate >= startDate)
                .Where(dp => dp.CreateDate <= endDate)
                .List();
        }

        public DealFromBitrix GetByBitrixId(IUnitOfWork uow, uint bitrixId)
        {
            return uow.Session.QueryOver<DealFromBitrix>()
                .Where(dp => dp.BitrixId == bitrixId)
                .SingleOrDefault();
        }

        public IList<DealFromBitrix> GetAllSuccessed(IUnitOfWork uow, DateTime startDate, DateTime endDate)
        {
            return uow.Session.QueryOver<DealFromBitrix>()
                .Where(dp => dp.Success == true)
                .Where(dp => dp.CreateDate >= startDate)
                .Where(dp => dp.CreateDate <= endDate)
                .List();
        }
    }
}