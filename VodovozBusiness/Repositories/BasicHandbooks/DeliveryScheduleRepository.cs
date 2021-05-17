using NHibernate.Criterion;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Repository
{
	public static class DeliveryScheduleRepository
	{
		public static QueryOver<DeliverySchedule> AllQuery ()
		{
			return QueryOver.Of<DeliverySchedule> ();
		}
		
		public static QueryOver<DeliverySchedule> NotArchiveQuery ()
		{
			return QueryOver.Of<DeliverySchedule>().WhereNot(ds => ds.IsArchive);
		}


		public static DeliverySchedule GetByBitrixId(IUnitOfWork uow, uint bitrixId)
		{
			return uow.Session.QueryOver<DeliverySchedule>()
				.Where(x => x.BitrixId == bitrixId)
				.SingleOrDefault();
		}
		
		public static IList<DeliverySchedule> All(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<DeliverySchedule>().List<DeliverySchedule>();
		}
	}
}