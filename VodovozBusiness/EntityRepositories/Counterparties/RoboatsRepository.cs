using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Osm.DTO;
using QS.Utilities.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Roboats;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public partial class RoboatsRepository
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public RoboatsRepository(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
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


		public int GetRoboatsCounterpartyNameId(int counterpartyId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				Counterparty counterpartyAlias = null;
				RoboAtsCounterpartyName counterpartyNameAlias = null;

				var query = uow.Session.QueryOver(() => counterpartyAlias)
					.Where(() => counterpartyAlias.Id == counterpartyId)
					.Select(Projections.Property(() => counterpartyAlias.FullName));

				var counterpartyName = query.SingleOrDefault<string>();

				PersonHelper.SplitFullName(counterpartyName, out string lastName, out string firstName, out string patronymic);
				if(string.IsNullOrWhiteSpace(firstName))
				{
					return 0;
				}

				var nameId = uow.Session.QueryOver(() => counterpartyNameAlias)
					.Where(() => counterpartyNameAlias.Name == firstName)
					.Select(Projections.Property(() => counterpartyNameAlias.Id))
					.SingleOrDefault<int>();

				return nameId;
			}
		}

		public int GetRoboatsCounterpartyPatronymicId(int counterpartyId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				Counterparty counterpartyAlias = null;
				RoboAtsCounterpartyPatronymic counterpartyPatronymicAlias = null;

				var query = uow.Session.QueryOver(() => counterpartyAlias)
					.Where(() => counterpartyAlias.Id == counterpartyId)
					.Select(Projections.Property(() => counterpartyAlias.FullName));

				var counterpartyName = query.SingleOrDefault<string>();

				PersonHelper.SplitFullName(counterpartyName, out string lastName, out string firstName, out string patronymic);
				if(string.IsNullOrWhiteSpace(patronymic))
				{
					return 0;
				}

				var nameId = uow.Session.QueryOver(() => counterpartyPatronymicAlias)
					.Where(() => counterpartyPatronymicAlias.Patronymic == patronymic)
					.Select(Projections.Property(() => counterpartyPatronymicAlias.Id))
					.SingleOrDefault<int>();

				return nameId;
			}
		}

		public Order GetLastOrder(int counterpartyId, int? deliveryPoint = null)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var acceptedOrderStatuses = new[] { OrderStatus.Shipped, OrderStatus.Closed, OrderStatus.UnloadingOnStock };

				Order orderAlias = null;

				var query = uow.Session.QueryOver(() => orderAlias)
					.Where(() => orderAlias.Client.Id == counterpartyId)
					.Where(() => orderAlias.DeliveryDate >= DateTime.Now.AddMonths(-4))
					.Where(
						Restrictions.In(
								Projections.Property(() => orderAlias.OrderStatus),
								acceptedOrderStatuses
						)
					);

				if(deliveryPoint.HasValue)
				{
					query.Where(() => orderAlias.DeliveryPoint.Id == deliveryPoint.Value);
				}

				query.OrderByAlias(() => orderAlias.CreateDate).Desc.Take(1);

				var result = query.SingleOrDefault<Order>();
				return result;
			}
		}

		public IEnumerable<int> GetRoboatsAvailableDeliveryIntervals()
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				DeliverySchedule deliveryScheduleAlias = null;

				var query = uow.Session.QueryOver(() => deliveryScheduleAlias)
					.Where(Restrictions.IsNotNull(Projections.Property(() => deliveryScheduleAlias.RoboatsId)))
					.Select(Projections.Property(() => deliveryScheduleAlias.RoboatsId));
				var result = query.List<int>();

				return result;
			}
		}

		public int GetLastOrderBottlesCount(int clientId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var acceptedOrderStatuses = new[] { OrderStatus.Shipped, OrderStatus.Closed, OrderStatus.UnloadingOnStock };

				Order orderAlias = null;
				OrderItem orderItemAlias = null;
				Nomenclature nomenclatureAlias = null;

				var orderSubQuery = QueryOver.Of(() => orderAlias)
					.Where(() => orderAlias.Client.Id == clientId)
					.Where(() => orderAlias.DeliveryDate >= DateTime.Now.AddMonths(-4))
					.Where(
						Restrictions.In(
								Projections.Property(() => orderAlias.OrderStatus),
								acceptedOrderStatuses
						)
					)
					.Select(x => x.Id)
					.OrderByAlias(() => orderAlias.CreateDate).Desc
					.Take(1);

				var query = uow.Session.QueryOver(() => orderItemAlias)
					.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
					.WithSubquery.WhereProperty(() => orderItemAlias.Order.Id).Eq(orderSubQuery)
					.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water)
					.Where(() => nomenclatureAlias.IsDisposableTare == false)
					.Select(
						Projections.Sum(
							Projections.Conditional(
								Restrictions.IsNull(Projections.Property(() => orderItemAlias.ActualCount)),
								Projections.Property(() => orderItemAlias.Count),
								Projections.Property(() => orderItemAlias.ActualCount)
							)
						)
					);

				var result = (int)(query.SingleOrDefault<decimal>());
				return result;
			}
		}

		public int GetLastOrderBottlesReturnCount(int clientId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var acceptedOrderStatuses = new[] { OrderStatus.Shipped, OrderStatus.Closed, OrderStatus.UnloadingOnStock };

				Order orderAlias = null;

				var query = uow.Session.QueryOver(() => orderAlias)
					.Where(() => orderAlias.Client.Id == clientId)
					.Where(() => orderAlias.DeliveryDate >= DateTime.Now.AddMonths(-4))
					.Where(
						Restrictions.In(
								Projections.Property(() => orderAlias.OrderStatus),
								acceptedOrderStatuses
						)
					)
					.Select(x => x.BottlesReturn)
					.OrderByAlias(() => orderAlias.CreateDate).Desc
					.Take(1);

				var result = query.SingleOrDefault<int>();
				return result;
			}
		}

		public IEnumerable<int> GetLastDeliveryPointIds(int clientId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var acceptedOrderStatuses = new[] { OrderStatus.Shipped, OrderStatus.Closed, OrderStatus.UnloadingOnStock };

				Order orderAlias = null;

				var query = uow.Session.QueryOver(() => orderAlias)
					.Where(() => orderAlias.Client.Id == clientId)
					.Where(() => orderAlias.DeliveryDate >= DateTime.Now.AddMonths(-4))
					.Where(
						Restrictions.In(
								Projections.Property(() => orderAlias.OrderStatus),
								acceptedOrderStatuses
						)
					)
					.Select(Projections.Distinct(Projections.Property(() => orderAlias.DeliveryPoint.Id)))
					.OrderByAlias(() => orderAlias.CreateDate).Desc;

				var result = query.List<int>();
				return result;
			}
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
					.Where(() => deliveryScheduleAlias.RoboatsId == roboatsTimeId);

				var result = query.SingleOrDefault();
				return result;
			}
		}

		public IEnumerable<RoboatsWaterType> GetAvailableWaters()
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{

				var availaleWaters = uow.GetAll<RoboatsWaterType>().ToList();
				/*Nomenclature nomenclatureAlias = null;
				RoboatsWaterNode resultAlias = null;

				var query = uow.Session.QueryOver(() => nomenclatureAlias)
					.Where(Restrictions.IsNotNull(Projections.Property(() => nomenclatureAlias.RoboatsId)))
					.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.NomenclatureId)
						.Select(x => x.RoboatsId).WithAlias(() => resultAlias.RoboatsId)
						.Select(x => x.RoboatsAudiofile).WithAlias(() => resultAlias.Audiofile)
					)
					.TransformUsing(Transformers.AliasToBean<RoboatsWaterNode>());

				var result = query.List<RoboatsWaterNode>();*/
				return availaleWaters;
			}
		}

		public IEnumerable<NomenclatureQuantity> GetWatersQuantityFromOrder(int counterpartyId, int orderId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var result = new List<NomenclatureQuantity>();

				var order = uow.GetById<Order>(orderId);

				if(order.Client.Id != counterpartyId)
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

		public string GetLastOrderDataAddressBuilding(int counterpartyId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var acceptedOrderStatuses = new[] { OrderStatus.Shipped, OrderStatus.Closed, OrderStatus.UnloadingOnStock };

				Order orderAlias = null;
				DeliveryPoint deliveryPointAlias = null;

				var query = uow.Session.QueryOver(() => orderAlias)
					.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
					.Where(() => orderAlias.Client.Id == counterpartyId)
					.Where(() => orderAlias.DeliveryDate >= DateTime.Now.AddMonths(-4))
					.Where(
						Restrictions.In(
								Projections.Property(() => orderAlias.OrderStatus),
								acceptedOrderStatuses
						)
					)
					.Select(Projections.Property(() => deliveryPointAlias.Building))
					.OrderByAlias(() => orderAlias.CreateDate).Desc
					.Take(1);

				var result = query.SingleOrDefault<string>();
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



		public string GetLastOrderDataAddressApartment(int counterpartyId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var acceptedOrderStatuses = new[] { OrderStatus.Shipped, OrderStatus.Closed, OrderStatus.UnloadingOnStock };

				Order orderAlias = null;
				DeliveryPoint deliveryPointAlias = null;

				var query = uow.Session.QueryOver(() => orderAlias)
					.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
					.Where(() => orderAlias.Client.Id == counterpartyId)
					.Where(() => orderAlias.DeliveryDate >= DateTime.Now.AddMonths(-4))
					.Where(
						Restrictions.In(
								Projections.Property(() => orderAlias.OrderStatus),
								acceptedOrderStatuses
						)
					)
					.Where(() => deliveryPointAlias.RoomType == RoomType.Apartment)
					.Select(Projections.Property(() => deliveryPointAlias.Room))
					.OrderByAlias(() => orderAlias.CreateDate).Desc
					.Take(1);

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

		public string GetLastOrderDataAddressOffice(int counterpartyId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var acceptedOrderStatuses = new[] { OrderStatus.Shipped, OrderStatus.Closed, OrderStatus.UnloadingOnStock };

				Order orderAlias = null;
				DeliveryPoint deliveryPointAlias = null;

				var query = uow.Session.QueryOver(() => orderAlias)
					.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
					.Where(() => orderAlias.Client.Id == counterpartyId)
					.Where(() => orderAlias.DeliveryDate >= DateTime.Now.AddMonths(-4))
					.Where(
						Restrictions.In(
								Projections.Property(() => orderAlias.OrderStatus),
								acceptedOrderStatuses
						)
					)
					.Where(() => deliveryPointAlias.RoomType == RoomType.Office)
					.Select(Projections.Property(() => deliveryPointAlias.Room))
					.OrderByAlias(() => orderAlias.CreateDate).Desc
					.Take(1);

				var result = query.SingleOrDefault<string>();
				return result;
			}
		}

		public string GetDeliveryPointOffice(int deliveryPointId, int counterpartyId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				DeliveryPoint deliveryPointAlias = null;

				var query = uow.Session.QueryOver(() => deliveryPointAlias)
					.Where(() => deliveryPointAlias.Id == deliveryPointId)
					.Where(() => deliveryPointAlias.Counterparty.Id == counterpartyId)
					.Where(() => deliveryPointAlias.RoomType == RoomType.Office)
					.Select(Projections.Property(() => deliveryPointAlias.Room));

				var result = query.SingleOrDefault<string>();
				return result;
			}
		}

		public RoomType GetDeliveryPointRoomType(int deliveryPointId, int counterpartyId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				DeliveryPoint deliveryPointAlias = null;

				var query = uow.Session.QueryOver(() => deliveryPointAlias)
					.Where(() => deliveryPointAlias.Id == deliveryPointId)
					.Where(() => deliveryPointAlias.Counterparty.Id == counterpartyId)
					.Select(Projections.Property(() => deliveryPointAlias.RoomType));

				var result = query.SingleOrDefault<RoomType>();
				return result;
			}
		}

		public RoomType GetLastOrderDeliveryPointRoomType(int counterpartyId)
		{
			throw new NotImplementedException();
		}

		public string GetCounterpartyFullName(int clientId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				Counterparty counterpartyAlias = null;

				var query = uow.Session.QueryOver(() => counterpartyAlias)
					.Where(() => counterpartyAlias.Id == clientId)
					.Select(Projections.Property(() => counterpartyAlias.FullName));

				var result = query.SingleOrDefault<string>();
				return result;
			}
		}
	}

	public class NomenclatureQuantity
	{
		public int NomenclatureId { get; }
		public int Quantity { get; }

		public NomenclatureQuantity(int nomenclatureId, int quantity)
		{
			NomenclatureId = nomenclatureId;
			Quantity = quantity;
		}
	}
}
