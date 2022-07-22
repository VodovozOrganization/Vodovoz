using NHibernate;
using NHibernate.Criterion;
using NLog;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Roboats;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories.Roboats
{
	public class RoboatsRepository : IRoboatsRepository
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly RoboatsSettings _roboatsSettings;
		private readonly INomenclatureParametersProvider _nomenclatureParametersProvider;

		public RoboatsRepository(IUnitOfWorkFactory unitOfWorkFactory, RoboatsSettings roboatsSettings, INomenclatureParametersProvider nomenclatureParametersProvider)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
			_nomenclatureParametersProvider = nomenclatureParametersProvider ?? throw new ArgumentNullException(nameof(nomenclatureParametersProvider));
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
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				T entityAlias = null;

				var query = uow.Session.QueryOver(() => entityAlias)
					.Where(Restrictions.IsNotNull(Projections.Property(() => entityAlias.RoboatsAudiofile)));
				return query.List().Cast<IRoboatsEntity>();
			}
		}

		public IEnumerable<int> GetCounterpartyIdsByPhone(string phone)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
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
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				return uow.GetAll<RoboatsWaterType>().ToList();
			}
		}

		public int GetRoboatsCounterpartyNameId(int counterpartyId, string phone)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
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
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
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
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				DeliverySchedule deliveryScheduleAlias = null;

				var query = uow.Session.QueryOver(() => deliveryScheduleAlias)
					.Where(Restrictions.IsNotNull(Projections.Property(() => deliveryScheduleAlias.RoboatsAudiofile)))
					.OrderBy(Projections.Property(() => deliveryScheduleAlias.From)).Asc();
				var result = query.List<DeliverySchedule>();

				return result;
			}
		}

		public IEnumerable<int> GetLastDeliveryPointIds(int clientId)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			var lastOrders = GetLastOrders(clientId);
			sw.Stop();
			return lastOrders.Select(o => o.DeliveryPoint.Id);
		}

		public Order GetLastOrder(int counterpartyId, int? deliveryPointId = null)
		{
			var lastOrders = GetLastOrders(counterpartyId);
			foreach(var order in lastOrders)
			{
				if(deliveryPointId.HasValue)
				{
					if(order.DeliveryPoint.Id == deliveryPointId.Value)
					{
						return order;
					}
					else
					{
						continue;
					}
				}
				else
				{
					return order;
				}
			}
			return null;
		}

		private IEnumerable<Order> GetLastOrders(int counterpartyId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var acceptedOrderStatuses = new[] { OrderStatus.Shipped, OrderStatus.Closed, OrderStatus.UnloadingOnStock };

				Order orderAlias = null;
				DeliveryPoint deliveryPointAlias = null;
				RoboatsFiasStreet roboatsFiasStreetAlias = null;
				PromotionalSet promotionalSetAlias = null;

				var lastOrdersByDeliveryPoints = uow.Session.QueryOver(() => orderAlias)
					.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
					.Left.JoinAlias(() => orderAlias.PromotionalSets, () => promotionalSetAlias)
					.JoinEntityQueryOver(() => roboatsFiasStreetAlias, Restrictions.Where(() => deliveryPointAlias.StreetFiasGuid == roboatsFiasStreetAlias.FiasStreetGuid))
					.Where(() => orderAlias.Client.Id == counterpartyId)
					.Where(() => orderAlias.DeliveryDate >= DateTime.Now.AddMonths(-4))
					.Where(() => !orderAlias.IsBottleStock)
					.Where(Restrictions.IsNull(Projections.Property(() => promotionalSetAlias.Id)))
					.Where(() => deliveryPointAlias.RoomType == RoomType.Apartment)
					.Where(
						Restrictions.In(
								Projections.Property(() => orderAlias.OrderStatus),
								acceptedOrderStatuses
						)
					)
					.Select(
						Projections.Max(Projections.Id()),
						Projections.GroupProperty(Projections.Property(() => orderAlias.DeliveryPoint.Id)))
					.List<object[]>().Select(x => (int)x[0]);

				OrderItem orderItemAlias = null;
				Nomenclature nomenclatureAlias = null;
				Counterparty counterpartyAlias = null;
				var orders = uow.Session.QueryOver(() => orderAlias)
					.Where(Restrictions.In(Projections.Property(() => orderAlias.Id), lastOrdersByDeliveryPoints.ToArray()))
					.Select(Projections.Distinct(Projections.RootEntity()))
					.OrderByAlias(() => orderAlias.Id).Desc()
					.List();

				var result = new List<Order>();
				foreach(var order in orders)
				{
					if(WasPassWaterCheck(order))
					{
						result.Add(order);
					}
				}
				return result;
			}
		}

		private IEnumerable<Order> GetLastOrders2(int counterpartyId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var acceptedOrderStatuses = new[] { OrderStatus.Shipped, OrderStatus.Closed, OrderStatus.UnloadingOnStock };

				Order orderAlias = null;
				DeliveryPoint deliveryPointAlias = null;
				RoboatsFiasStreet roboatsFiasStreetAlias = null;
				PromotionalSet promotionalSetAlias = null;

				var lastOrdersByDeliveryPoints = uow.Session.QueryOver(() => orderAlias)
					.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
					.Left.JoinAlias(() => orderAlias.PromotionalSets, () => promotionalSetAlias)
					.JoinEntityQueryOver(() => roboatsFiasStreetAlias, Restrictions.Where(() => deliveryPointAlias.StreetFiasGuid == roboatsFiasStreetAlias.FiasStreetGuid))
					.Where(() => orderAlias.Client.Id == counterpartyId)
					.Where(() => orderAlias.DeliveryDate >= DateTime.Now.AddMonths(-4))
					.Where(() => !orderAlias.IsBottleStock)
					.Where(Restrictions.IsNull(Projections.Property(() => promotionalSetAlias.Id)))
					.Where(() => deliveryPointAlias.RoomType == RoomType.Apartment)
					.Where(
						Restrictions.In(
								Projections.Property(() => orderAlias.OrderStatus),
								acceptedOrderStatuses
						)
					)
					.Select(
						Projections.Max(Projections.Id()),
						Projections.GroupProperty(Projections.Property(() => orderAlias.DeliveryPoint.Id)))
					.List<object[]>().Select(x => (int)x[0]);

				OrderItem orderItemAlias = null;
				Nomenclature nomenclatureAlias = null;
				Counterparty counterpartyAlias = null;
				/*var orders = uow.Session.QueryOver(() => orderAlias)
					//.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
					//.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias).Fetch(SelectMode.Fetch, () => orderItemAlias)
					//.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias).Fetch(SelectMode.Fetch, () => nomenclatureAlias)
					//.Fetch(SelectMode.ChildFetch, () => orderAlias.OrderItems)
					.Where(Restrictions.In(Projections.Property(() => orderAlias.Id), lastOrdersByDeliveryPoints.ToArray()))
					.Select(Projections.Distinct(Projections.RootEntity()))
					.OrderByAlias(() => orderAlias.Id).Desc()
					.Future<Order>();*/

				var orders1 = uow.Session.QueryOver(() => orderAlias)
					.Where(Restrictions.In(Projections.Property(() => orderAlias.Id), lastOrdersByDeliveryPoints.ToArray()))
					.Future<Order>();

				/*var orders2 = uow.Session.QueryOver(() => orderAlias)
					.Fetch(SelectMode.Fetch, () => orderAlias.DeliveryPoint.DefaultWaterNomenclature.DependsOnNomenclature.Unit)
					.Where(Restrictions.In(Projections.Property(() => orderAlias.Id), lastOrdersByDeliveryPoints.ToArray()))
					.Future<Order>();

				var orders3 = uow.Session.QueryOver(() => orderAlias)
					.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
					.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
					.Fetch(SelectMode.Fetch, () => nomenclatureAlias.Unit)
					.Where(Restrictions.In(Projections.Property(() => orderAlias.Id), lastOrdersByDeliveryPoints.ToArray()))
					.Future<Order>();

				var orders4 = uow.Session.QueryOver(() => orderAlias)
					.Fetch(SelectMode.Fetch, () => orderAlias.Contract)
					.Where(Restrictions.In(Projections.Property(() => orderAlias.Id), lastOrdersByDeliveryPoints.ToArray()))
					.Future<Order>();

				var orders5 = uow.Session.QueryOver(() => orderAlias)
					.Fetch(SelectMode.Fetch, () => orderAlias.DeliveryPoint.Category)
					.Where(Restrictions.In(Projections.Property(() => orderAlias.Id), lastOrdersByDeliveryPoints.ToArray()))
					.Future<Order>();*/

				Stopwatch sw = new Stopwatch();
				sw.Start();
				var orders = orders1.ToList();
				sw.Stop();
				logger.Debug($"Загружено за {sw.ElapsedMilliseconds}ms");

				var orderItemsCount = orders[0].OrderItems[0].Count;

				var result = new List<Order>();
				foreach(var order in orders)
				{
					if(WasPassWaterCheck(order))
					{
						result.Add(order);
					}
				}
				return result;
			}
		}

		private bool WasPassWaterCheck(Order order)
		{
			var hasOnlyWater = !order.OrderItems
				.Where(x => x.Nomenclature.Id != _nomenclatureParametersProvider.PaidDeliveryNomenclatureId)
				.Any(x => x.Nomenclature.Category != NomenclatureCategory.water);

			if(!hasOnlyWater)
			{
				return false;
			}

			var hasWaterRowDuplicate = order.OrderItems.GroupBy(x => x.Nomenclature.Id).Any(x => x.Count() > 1);
			return !hasWaterRowDuplicate;
		}

		public int? GetBottlesReturnForOrder(int counterpartyId, int orderId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
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
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
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
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
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

		public int? GetRoboAtsStreetId(int counterPartyId, int deliveryPointId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
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
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
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
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				DeliveryPoint deliveryPointAlias = null;

				var query = uow.Session.QueryOver(() => deliveryPointAlias)
					.Where(() => deliveryPointAlias.Id == deliveryPointId)
					.Where(() => deliveryPointAlias.Counterparty.Id == counterpartyId)
					.Where(() => deliveryPointAlias.RoomType == RoomType.Apartment)
					.Select(Projections.Property(() => deliveryPointAlias.Room));

				var result = query.SingleOrDefault<string>();
				return result;
			}
		}
	}
}
