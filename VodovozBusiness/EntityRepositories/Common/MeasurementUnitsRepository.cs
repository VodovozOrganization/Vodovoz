using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.Common;
using NHibernate.Criterion;

namespace Vodovoz.EntityRepositories.Common
{
    public class MeasurementUnitsRepository : IMeasurementUnitsRepository
	{
		public IList<MeasurementUnit> GetActiveUnits(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<MeasurementUnit>().List();
		}

		public MeasurementUnit GetDefaultGoodsUnit(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<MeasurementUnit>()
				.Where(n => n.Name.IsLike("шт%"))
				.Take(1).SingleOrDefault();
		}

		public MeasurementUnit GetDefaultGoodsService(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<MeasurementUnit>()
				.Where(n => n.Name.IsLike("усл%"))
				.Take(1).SingleOrDefault();
		}

		public MeasurementUnit GetUnitsByOKEI(IUnitOfWork uow, string okei)
		{
			return uow.Session.QueryOver<MeasurementUnit>()
				.Where(n => n.OKEI == okei)
				.Take(1).SingleOrDefault();
		}

		public MeasurementUnit GetUnitsByBitrix(IUnitOfWork uow, string bitrixName)
		{
			return uow.Session.QueryOver<MeasurementUnit>()
			  .Where(n => n.BitrixName.IsLike(bitrixName))
			  .Take(1).SingleOrDefault();
		}
	}
}
