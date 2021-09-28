using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.Common;

namespace Vodovoz.EntityRepositories.Common
{
    public interface IMeasurementUnitsRepository
    {
        IList<MeasurementUnit> GetActiveUnits(IUnitOfWork uow);
        MeasurementUnit GetDefaultGoodsUnit(IUnitOfWork uow);
        MeasurementUnit GetDefaultGoodsService(IUnitOfWork uow);
        MeasurementUnit GetUnitsByOKEI(IUnitOfWork uow, string okei);
        MeasurementUnit GetUnitsByBitrix(IUnitOfWork uow, string bitrixName);
    }
}
