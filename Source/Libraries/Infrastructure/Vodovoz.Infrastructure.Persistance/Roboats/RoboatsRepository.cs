using NHibernate;
using NHibernate.Criterion;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QS.Project.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using QS.Project.DB;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Roboats;
using Vodovoz.Settings.Roboats;
using Vodovoz.EntityRepositories.Roboats;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Infrastructure.Persistance.Roboats
{
	internal sealed class RoboatsRepository : IRoboatsRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IRoboatsSettings _roboatsSettings;

		private HashSet<Guid> _roboatsStreetsCache = new HashSet<Guid>();
		private int _roboatsStreetsCacheTimeoutMinutes = 10;
		private DateTime _roboatsStreetsCacheLastUpdate;

		private IEnumerable<RoboatsWaterType> _roboatsWatersCache = Enumerable.Empty<RoboatsWaterType>();
		private int _roboatsWatersCacheTimeoutMinutes = 10;
		private DateTime _roboatsWatersCacheLastUpdate;


		public RoboatsRepository(IUnitOfWorkFactory uowFactory, IRoboatsSettings roboatsSettings)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
		}

		public IEnumerable<IRoboatsEntity> GetExportedEntities(RoboatsEntityType roboatsEntityType)
		{
			switch(roboatsEntityType)
			{
				case RoboatsEntityType.DeliverySchedules:
					return GetRoboatsEntity<DeliverySchedule>();
				case RoboatsEntityType.Streets:
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
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				T entityAlias = null;

				var query = uow.Session.QueryOver(() => entityAlias)
					.Where(Restrictions.IsNotNull(Projections.Property(() => entityAlias.RoboatsAudiofile)));
				return query.List().Cast<IRoboatsEntity>();
			}
		}

		public IEnumerable<int> GetCounterpartyIdsByPhone(string phone)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				Phone phoneAlias = null;
				Counterparty counterpartyAlias = null;
				Counterparty counterpartyFromDeliveryPointAlias = null;
				DeliveryPoint deliveryPointAlias = null;

				var query = uow.Session.QueryOver(() => phoneAlias)
					.Left.JoinAlias(() => phoneAlias.Counterparty, () => counterpartyAlias)
					.Left.JoinAlias(() => phoneAlias.DeliveryPoint, () => deliveryPointAlias)
					.Left.JoinAlias(() => deliveryPointAlias.Counterparty, () => counterpartyFromDeliveryPointAlias)
					.Where(() => phoneAlias.DigitsNumber == phone)
					.Where(
						Restrictions.Or(
							Restrictions.And(
								Restrictions.IsNotNull(Projections.Property(() => phoneAlias.Counterparty)),
								Restrictions.Where(() => counterpartyAlias.PersonType == PersonType.natural)
							),
							Restrictions.And(
								Restrictions.IsNotNull(Projections.Property(() => phoneAlias.DeliveryPoint)),
								Restrictions.Where(() => counterpartyFromDeliveryPointAlias.PersonType == PersonType.natural)
							)
						)
					)
					.Select(
						Projections.Distinct(
							Projections.Conditional(
								Restrictions.IsNull(Projections.Property(() => counterpartyAlias.Id)),
								Projections.Property(() => deliveryPointAlias.Counterparty.Id),
								Projections.Property(() => counterpartyAlias.Id)
							)
						)
					);

				var result = query.List<int>();
				return result;
			}
		}

		public IEnumerable<RoboatsWaterType> GetWaterTypes()
		{
			var timeExpired = DateTime.Now - _roboatsWatersCacheLastUpdate;
			if(timeExpired.TotalMinutes <= _roboatsWatersCacheTimeoutMinutes)
			{
				return _roboatsWatersCache;
			}

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				RoboatsWaterType roboatsWaterTypeAlias = null;
				var waters = uow.Session.QueryOver(() => roboatsWaterTypeAlias)
					.OrderBy(() => roboatsWaterTypeAlias.Order).Asc()
					.List();
				_roboatsWatersCache = waters;
				return _roboatsWatersCache;
			}
		}

		public int GetRoboatsCounterpartyNameId(int counterpartyId, string phone)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var resultPhone = GetCountepartyPhone(uow, phone, counterpartyId);

				if(resultPhone == null || resultPhone.RoboAtsCounterpartyName.Id == _roboatsSettings.DefaultCounterpartyNameId)
				{
					resultPhone = GetDeliveryPointPhone(uow, phone, counterpartyId);
				}

				if(resultPhone == null)
				{
					return _roboatsSettings.DefaultCounterpartyNameId;
				}

				return resultPhone.RoboAtsCounterpartyName.Id;
			}
		}

		public int GetRoboatsCounterpartyPatronymicId(int counterpartyId, string phone)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var resultPhone = GetCountepartyPhone(uow, phone, counterpartyId);

				if(resultPhone == null || resultPhone.RoboAtsCounterpartyPatronymic.Id == _roboatsSettings.DefaultCounterpartyPatronymicId)
				{
					resultPhone = GetDeliveryPointPhone(uow, phone, counterpartyId);
				}

				if(resultPhone == null)
				{
					return _roboatsSettings.DefaultCounterpartyPatronymicId;
				}

				return resultPhone.RoboAtsCounterpartyPatronymic.Id;
			}
		}

		private Phone GetCountepartyPhone(IUnitOfWork uow, string phoneNumber, int counterpartyId)
		{
			Phone phoneAlias = null;

			var query = uow.Session.QueryOver(() => phoneAlias)
				.Where(() => phoneAlias.DigitsNumber == phoneNumber)
				.Where(() => phoneAlias.Counterparty.Id == counterpartyId)
				.Where(Restrictions.IsNotNull(Projections.Property(() => phoneAlias.Counterparty)))
				.Where(Restrictions.IsNotNull(Projections.Property(() => phoneAlias.RoboAtsCounterpartyName)))
				.OrderBy(Projections.Id()).Desc();

			Phone resultPhone = null;

			var counterpartyPhones = query.List<Phone>();
			foreach(var phone in counterpartyPhones)
			{
				resultPhone = phone;
				if(resultPhone.RoboAtsCounterpartyName.Id != _roboatsSettings.DefaultCounterpartyNameId)
				{
					continue;
				}
			}
			return resultPhone;
		}

		private Phone GetDeliveryPointPhone(IUnitOfWork uow, string phoneNumber, int counterpartyId)
		{
			Phone phoneAlias = null;
			DeliveryPoint deliveryPointAlias = null;

			var query = uow.Session.QueryOver(() => phoneAlias)
				.Left.JoinAlias(() => phoneAlias.DeliveryPoint, () => deliveryPointAlias)
				.Where(() => phoneAlias.DigitsNumber == phoneNumber)
				.Where(() => deliveryPointAlias.Counterparty.Id == counterpartyId)
				.Where(Restrictions.IsNotNull(Projections.Property(() => phoneAlias.DeliveryPoint)))
				.Where(Restrictions.IsNotNull(Projections.Property(() => phoneAlias.RoboAtsCounterpartyName)))
				.OrderBy(Projections.Id()).Desc();

			Phone resultPhone = null;

			var phones = query.List<Phone>();
			foreach(var phone in phones)
			{
				resultPhone = phone;
				if(resultPhone.RoboAtsCounterpartyName.Id != _roboatsSettings.DefaultCounterpartyNameId)
				{
					continue;
				}
			}
			return resultPhone;
		}

		public IEnumerable<DeliverySchedule> GetRoboatsAvailableDeliveryIntervals()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				DeliverySchedule deliveryScheduleAlias = null;

				var query = uow.Session.QueryOver(() => deliveryScheduleAlias)
					.Where(Restrictions.IsNotNull(Projections.Property(() => deliveryScheduleAlias.RoboatsAudiofile)))
					.OrderBy(Projections.Property(() => deliveryScheduleAlias.From)).Asc();
				var result = query.List<DeliverySchedule>();

				return result;
			}
		}

		public IEnumerable<RoboatsDeliveryIntervalRestriction> GetRoboatsDeliveryIntervalRestrictions()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var result = uow.GetAll<RoboatsDeliveryIntervalRestriction>().ToList();
				return result;
			}
		}


		public IEnumerable<Order> GetLastOrders(int clientId)
		{
			return GetLastOrders(clientId, null);
		}

		public Order GetLastOrder(int clientId, int? deliveryPointId = null)
		{
			var lastOrders = GetLastOrders(clientId, deliveryPointId);
			return lastOrders.FirstOrDefault();
		}

		private IEnumerable<Order> GetLastOrders(int counterpartyId, int? deliveryPointId = null)
		{
			IList<Order> orders = new List<Order>();
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				Order orderAlias = null;
				OrderItem orderItemAlias = null;
				Nomenclature nomenclatureAlias = null;
				MeasurementUnits measurementUnitsAlias = null;

				var lastOrdersByDeliveryPoints = uow.Session.QueryOver(() => orderAlias)
					.Where(() => orderAlias.Client.Id == counterpartyId)
					.Where(Restrictions.IsNotNull(Projections.Property(() => orderAlias.DeliveryPoint)))
					.Select(
						Projections.Max(Projections.Id()),
						Projections.Group(() => orderAlias.DeliveryPoint.Id)
					)
					.List<object[]>()
					.Select(x => (int)x[0])
				;

				IQueryOver<Order, Order> CreateLastOrdersBaseQuery()
				{
					var baseQuery = uow.Session.QueryOver(() => orderAlias)
						.Where(Restrictions.In(Projections.Property(() => orderAlias.Id), lastOrdersByDeliveryPoints.ToArray()));
					if(deliveryPointId.HasValue)
					{
						baseQuery.Where(() => orderAlias.DeliveryPoint.Id == deliveryPointId.Value);
					}
					return baseQuery;
				}

				// Порядок составления запросов важен.
				// Запрос построен так, чтобы в одном обращении к базе были загружены
				// все поля, используемые валидаторами Roboats

				var ordersQuery = CreateLastOrdersBaseQuery()
					.Future<Order>();

				var measurementUnitsQuery = uow.Session.QueryOver(() => measurementUnitsAlias)
					.Future<MeasurementUnits>();

				var orderItemsQuery = CreateLastOrdersBaseQuery()
					.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
					.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
					.Fetch(SelectMode.Fetch, () => nomenclatureAlias.Unit)
					.Future<Order>();

				var counterpartyQuery = CreateLastOrdersBaseQuery()
					.Fetch(SelectMode.Fetch, () => orderAlias.Client)
					.Future<Order>();

				var promosetsQuery = CreateLastOrdersBaseQuery()
					.Fetch(SelectMode.Fetch, () => orderAlias.PromotionalSets)
					.Future<Order>();

				var contractQuery = CreateLastOrdersBaseQuery()
					.Fetch(SelectMode.Fetch, () => orderAlias.Contract)
					.Future<Order>();

				var deliveryPointQuery = CreateLastOrdersBaseQuery()
					.Fetch(SelectMode.Fetch, () => orderAlias.DeliveryPoint)
					.Future<Order>();

				orders = deliveryPointQuery.ToList();
			}

			return orders;
		}

		public int? GetBottlesReturnForOrder(int counterpartyId, int orderId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var order = uow.GetById<Order>(orderId);
				if(order == null)
				{
					return null;
				}

				if(order.Client.Id != counterpartyId)
				{
					return null;
				}

				return order.BottlesReturn ?? 0;
			}
		}

		public DeliverySchedule GetDeliverySchedule(int roboatsTimeId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				DeliverySchedule deliveryScheduleAlias = null;

				var query = uow.Session.QueryOver(() => deliveryScheduleAlias)
					.Where(() => deliveryScheduleAlias.Id == roboatsTimeId);

				var result = query.SingleOrDefault();
				return result;
			}
		}

		public IEnumerable<NomenclatureQuantity> GetWatersQuantityFromOrder(int counterpartyId, int orderId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var result = new List<NomenclatureQuantity>();

				var order = uow.GetById<Order>(orderId);
				if(order == null || order.Client.Id != counterpartyId)
				{
					return result;
				}

				var waterItems = order.OrderItems
					.Where(x => x.Nomenclature.Category == NomenclatureCategory.water)
					.Where(x => x.Nomenclature.TareVolume == TareVolume.Vol19L)
					.Where(x => x.Nomenclature.IsDisposableTare == false);

				foreach(var waterItem in waterItems)
				{
					NomenclatureQuantity nq = new NomenclatureQuantity(waterItem.Nomenclature.Id, (int)waterItem.CurrentCount);
					result.Add(nq);
				}

				return result;
			}
		}

		public HashSet<Guid> GetAvailableForRoboatsFiasStreetGuids()
		{
			var timeExpired = DateTime.Now - _roboatsStreetsCacheLastUpdate;
			if(timeExpired.TotalMinutes <= _roboatsStreetsCacheTimeoutMinutes)
			{
				return _roboatsStreetsCache;
			}

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				RoboatsStreet roboatsStreetAlias = null;
				RoboatsFiasStreet roboatsFiasStreetAlias = null;

				var query = uow.Session.QueryOver(() => roboatsStreetAlias)
					.JoinEntityQueryOver(() => roboatsFiasStreetAlias, Restrictions.Where(() => roboatsStreetAlias.Id == roboatsFiasStreetAlias.RoboatsAddress.Id))
					.Where(Restrictions.IsNotNull(Projections.Property(() => roboatsStreetAlias.FileId)))
					.SelectList(list => list
						.Select(() => roboatsFiasStreetAlias.FiasStreetGuid)
					);
				_roboatsStreetsCache = new HashSet<Guid>(query.List<Guid>());
				return _roboatsStreetsCache;
			}
		}

		public int? GetRoboAtsStreetId(int counterPartyId, int deliveryPointId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				RoboatsStreet roboatsStreetAlias = null;
				RoboatsFiasStreet roboatsFiasStreetAlias = null;
				DeliveryPoint deliveryPointAlias = null;

				var subQuery = QueryOver.Of(() => deliveryPointAlias)
					.Where(() => deliveryPointAlias.Id == deliveryPointId)
					.Where(() => deliveryPointAlias.Counterparty.Id == counterPartyId)
					.Select(Projections.Property(() => deliveryPointAlias.StreetFiasGuid));

				var query = uow.Session.QueryOver(() => roboatsFiasStreetAlias)
					.Left.JoinAlias(() => roboatsFiasStreetAlias.RoboatsAddress, () => roboatsStreetAlias)
					.WithSubquery.Where(() => roboatsFiasStreetAlias.FiasStreetGuid == subQuery.As<Guid>())
					.Select(Projections.Property(() => roboatsStreetAlias.Id));

				var result = query.SingleOrDefault<int?>();

				return result;
			}
		}

		public string GetDeliveryPointBuilding(int deliveryPointId, int counterpartyId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				DeliveryPoint deliveryPointAlias = null;

				var query = uow.Session.QueryOver(() => deliveryPointAlias)
					.Where(() => deliveryPointAlias.Id == deliveryPointId)
					.Where(() => deliveryPointAlias.Counterparty.Id == counterpartyId)
					.Select(Projections.Property(() => deliveryPointAlias.Building));

				var result = query.SingleOrDefault<string>();
				return result;
			}
		}

		public string GetDeliveryPointApartment(int deliveryPointId, int counterpartyId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				DeliveryPoint deliveryPointAlias = null;

				var query = uow.Session.QueryOver(() => deliveryPointAlias)
					.Where(() => deliveryPointAlias.Id == deliveryPointId)
					.Where(() => deliveryPointAlias.Counterparty.Id == counterpartyId)
					.Select(Projections.Property(() => deliveryPointAlias.Room));

				var result = query.SingleOrDefault<string>();
				return result;
			}
		}

		public bool CounterpartyExcluded(int counterpartyId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				Counterparty counterpartyAlias = null;
				var query = uow.Session.QueryOver(() => counterpartyAlias)
					.Where(() => counterpartyAlias.Id == counterpartyId)
					.Select(Projections.Property(() => counterpartyAlias.RoboatsExclude));
				var result = query.SingleOrDefault<bool>();
				return result;
			}
		}

		public IEnumerable<RoboatsCall> GetStaleCalls(IUnitOfWork uow)
		{
			RoboatsCall roboatsCallAlias = null;
			var staleCalls = uow.Session.QueryOver(() => roboatsCallAlias)
				.Where(() => roboatsCallAlias.CallTime < DateTime.Now.AddMinutes(-_roboatsSettings.CallTimeout))
				.Where(() => roboatsCallAlias.Status == RoboatsCallStatus.InProgress)
				.List();
			return staleCalls;
		}

		public RoboatsCall GetCall(IUnitOfWork uow, Guid callGuid)
		{
			RoboatsCall roboatsCallAlias = null;
			var call = uow.Session.QueryOver(() => roboatsCallAlias)
				.Where(() => roboatsCallAlias.CallGuid == callGuid)
				.SingleOrDefault();
			return call;
		}

		public RoboAtsCounterpartyName GetCounterpartyName(IUnitOfWork uow, string name)
		{
			if(string.IsNullOrWhiteSpace(name))
			{
				return null;
			}

			return uow.Session.QueryOver<RoboAtsCounterpartyName>()
				.Where(Restrictions.Eq(
					CustomProjections.Lower<RoboAtsCounterpartyName>(rn => rn.Name),
					name.ToLower()))
				.Take(1)
				.SingleOrDefault();
		}

		public RoboAtsCounterpartyPatronymic GetCounterpartyPatronymic(IUnitOfWork uow, string patronymic)
		{
			if(string.IsNullOrWhiteSpace(patronymic))
			{
				return null;
			}

			return uow.Session.QueryOver<RoboAtsCounterpartyPatronymic>()
				.Where(Restrictions.Eq(
					CustomProjections.Lower<RoboAtsCounterpartyPatronymic>(rp => rp.Patronymic),
					patronymic.ToLower()))
				.Take(1)
				.SingleOrDefault();
		}

		public IEnumerable<TodayIntervalOffer> GetTodayIntervalsOffers()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				return uow.GetAll<TodayIntervalOffer>().ToList();
			}
		}
	}
}
