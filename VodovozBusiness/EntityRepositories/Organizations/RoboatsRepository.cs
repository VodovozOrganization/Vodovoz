using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.EntityRepositories.Organizations
{
	public class RoboatsRepository : IRoboatsRepository
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public RoboatsRepository(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

		public IEnumerable<DeliverySchedule> GetDeliverySchedulesForRoboats()
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				DeliverySchedule deliveryScheduleAlias = null;

				var query = uow.Session.QueryOver(() => deliveryScheduleAlias)
					.Where(Restrictions.IsNotNull(Projections.Property(() => deliveryScheduleAlias.RoboatsAudiofile)));
				var result = query.List();
				return result;
			}
		}

		public IEnumerable<IRoboatsEntity> GetExportedEntities(RoboatsEntityType roboatsEntityType)
		{
			switch(roboatsEntityType)
			{
				case RoboatsEntityType.DeliverySchedules:
					return GetRoboatsEntity<DeliverySchedule>();
				case RoboatsEntityType.Street:
					return GetRoboatsEntity<RoboatsStreet>();
				case RoboatsEntityType.WaterTypes:
					return GetRoboatsEntity<RoboatsWaterType>();
				case RoboatsEntityType.CounterpartyName:
					return GetRoboatsEntity<RoboAtsCounterpartyName>();
				case RoboatsEntityType.CounterpartyPatronymic:
					return GetRoboatsEntity<RoboAtsCounterpartyPatronymic>();
				default:
					throw new NotSupportedException($"Тип {roboatsEntityType} не поддерживается");
			}
		}

		private IEnumerable<IRoboatsEntity> GetRoboatsEntity<T>()
			where T : class, IRoboatsEntity
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				T entityAlias = null;

				var query = uow.Session.QueryOver(() => entityAlias)
					.Where(Restrictions.IsNotNull(Projections.Property(() => entityAlias.RoboatsAudiofile)));
				return query.List().Cast<IRoboatsEntity>();
			}
		}

		public IEnumerable<Nomenclature> GetWaterTypesForRoboats()
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				Nomenclature nomenclatureAlias = null;

				var query = uow.Session.QueryOver(() => nomenclatureAlias)
					.Where(Restrictions.IsNotNull(Projections.Property(() => nomenclatureAlias.RoboatsAudiofile)));
				var result = query.List();
				return result;
			}
		}
	}
}
