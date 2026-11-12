using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.BasicHandbooks;

namespace Vodovoz.Infrastructure.Persistance.BasicHandbooks
{
	internal sealed class DeliveryScheduleRepository : IDeliveryScheduleRepository
	{
		public QueryOver<DeliverySchedule> AllQuery()
		{
			return QueryOver.Of<DeliverySchedule>();
		}

		public QueryOver<DeliverySchedule> NotArchiveQuery()
		{
			return QueryOver.Of<DeliverySchedule>().WhereNot(ds => ds.IsArchive);
		}

		public IList<DeliverySchedule> All(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<DeliverySchedule>().List<DeliverySchedule>();
		}

		public DeliverySchedule Get(IUnitOfWork uow, int deliveryScheduleId)
		{
			return (
				from deliverySchedule in uow.Session.Query<DeliverySchedule>()
				where deliverySchedule.Id == deliveryScheduleId
				select deliverySchedule
				)
				.FirstOrDefault();
		}

		public bool Exists(IUnitOfWork uow, int deliveryScheduleId)
		{
			return uow.Session.Query<DeliverySchedule>()
				.Any(x => x.Id == deliveryScheduleId);
		}

		public bool Available(
			IUnitOfWork uow,
			DayOfWeek weekDay,
			int deliveryPointId,
			int deliveryScheduleId
			)
		{
			return (
				from district in uow.Session.Query<District>()
				join deliveryPoint in uow.Session.Query<DeliveryPoint>()
					on district.Id equals deliveryPoint.District.Id
				join deliveryScheduleRestriction in uow.Session.Query<DeliveryScheduleRestriction>()
					on district.Id equals deliveryScheduleRestriction.District.Id
				join deliverySchedule in uow.Session.Query<DeliverySchedule>()
					on deliveryScheduleRestriction.DeliverySchedule.Id equals deliverySchedule.Id
				where deliveryPoint.Id == deliveryPointId
					&& deliverySchedule.Id == deliveryScheduleId
					&& !deliverySchedule.IsArchive
					&& deliveryScheduleRestriction.WeekDay == weekDay.ConvertToWeekDayName()
				select deliverySchedule.Id
			).Any();
		}
	}
}
