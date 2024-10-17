using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Infrastructure.Persistance.Counterparties
{
	internal sealed class DeliveryPointRepository : IDeliveryPointRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public DeliveryPointRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		/// <summary>
		/// Запрос ищет точку доставки в контрагенте по коду 1с или целиком по адресной строке.
		/// </summary>
		public DeliveryPoint GetByAddress1c(IUnitOfWork uow, Counterparty counterparty, string address1cCode, string address1c)
		{
			if(string.IsNullOrWhiteSpace(address1c) || counterparty != null)
			{
				return null;
			}

			return uow.Session.QueryOver<DeliveryPoint>()
					  .Where(x => x.Counterparty.Id == counterparty.Id)
					  .Where(dp => dp.Code1c != null && dp.Code1c == address1cCode || dp.Address1c == address1c)
					  .Take(1)
					  .SingleOrDefault();
		}

		public int GetBottlesOrderedForPeriod(IUnitOfWork uow, DeliveryPoint deliveryPoint, DateTime start, DateTime end)
		{
			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			var notConfirmedQueryResult = uow.Session.QueryOver(() => orderAlias)
				.Where(() => orderAlias.DeliveryPoint.Id == deliveryPoint.Id)
				.Where(() => start < orderAlias.DeliveryDate && orderAlias.DeliveryDate < end)
				.Where(() => orderAlias.OrderStatus != OrderStatus.Canceled)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && !nomenclatureAlias.IsDisposableTare)
				.Select(Projections.Sum(() => orderItemAlias.Count)).List<decimal?>();

			var confirmedQueryResult = uow.Session.QueryOver(() => orderAlias)
				.Where(() => orderAlias.DeliveryPoint.Id == deliveryPoint.Id)
				.Where(() => start < orderAlias.DeliveryDate && orderAlias.DeliveryDate < end)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Closed)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && !nomenclatureAlias.IsDisposableTare)
				.Where(() => orderItemAlias.ActualCount != null)
				.Select(Projections.Sum(() => orderItemAlias.ActualCount)).List<decimal?>();

			var bottlesOrdered = notConfirmedQueryResult.FirstOrDefault().GetValueOrDefault()
				+ confirmedQueryResult.FirstOrDefault().GetValueOrDefault();

			return (int)bottlesOrdered;
		}

		public decimal GetAvgBottlesOrdered(IUnitOfWork uow, DeliveryPoint deliveryPoint, int? countLastOrders)
		{
			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			var confirmedQueryResult = uow.Session.QueryOver(() => orderAlias)
				.Where(() => orderAlias.DeliveryPoint.Id == deliveryPoint.Id)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Closed)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && !nomenclatureAlias.IsDisposableTare)
				.OrderByAlias(() => orderAlias.DeliveryDate).Desc;

			if(countLastOrders.HasValue)
			{
				confirmedQueryResult.Take(countLastOrders.Value);
			}

			var list = confirmedQueryResult.Select(Projections.Group<Order>(x => x.Id),
				Projections.Sum(() => orderItemAlias.Count)).List<object[]>();

			return list.Count > 0 ? list.Average(x => (decimal)x[1]) : 0;
		}

		public IOrderedEnumerable<DeliveryPointCategory> GetActiveDeliveryPointCategories(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<DeliveryPointCategory>().Where(c => !c.IsArchive).List().OrderBy(c => c.Name);
		}

		public IList<DeliveryPoint> GetDeliveryPointsByCounterpartyId(IUnitOfWork uow, int counterpartyId)
		{
			var result = uow.Session.QueryOver<DeliveryPoint>()
				.Where(dp => dp.Counterparty.Id == counterpartyId)
				.List<DeliveryPoint>();

			return result;
		}

		public IEnumerable<string> GetAddressesWithFixedPrices(int counterpartyId)
		{
			IEnumerable<string> result;
			using(var uow = _uowFactory.CreateWithoutRoot($"Получение списка адресов имеющих фиксированную цену"))
			{
				DeliveryPoint deliveryPointAlias = null;
				NomenclatureFixedPrice fixedPriceAlias = null;

				result = uow.Session.QueryOver(() => fixedPriceAlias)
					.Inner.JoinAlias(() => fixedPriceAlias.DeliveryPoint, () => deliveryPointAlias)
					.Where(() => deliveryPointAlias.Counterparty.Id == counterpartyId)
					.SelectList(list => list.SelectGroup(() => deliveryPointAlias.ShortAddress))
					.List<string>();
			}

			return result;
		}

		public bool CheckingAnAddressForDeliveryForNewCustomers(IUnitOfWork uow, DeliveryPoint deliveryPoint)
		{
			string building = GetBuildingNumber(deliveryPoint.Building);
			DeliveryPoint deliveryPointAlias = null;
			Counterparty counterpartyAlias = null;

			var result = uow.Session.QueryOver(() => deliveryPointAlias)
									.JoinAlias(() => deliveryPointAlias.Counterparty, () => counterpartyAlias)
									.Where(() => deliveryPointAlias.City.IsLike(deliveryPoint.City, MatchMode.Anywhere)
											  && deliveryPointAlias.Street.IsLike(deliveryPoint.Street, MatchMode.Anywhere)
											  && deliveryPointAlias.Building.IsLike(building, MatchMode.Anywhere)
											  && deliveryPointAlias.Room == deliveryPoint.Room
											  && deliveryPointAlias.Id != deliveryPoint.Id)
									.List<DeliveryPoint>();

			return result.Count() == 0;
		}

		public IEnumerable<DeliveryPointForSendNode> GetActiveDeliveryPointsForSendByCounterpartyId(IUnitOfWork uow, int counterpartyId)
		{
			DeliveryPointForSendNode resultAlias = null;

			var result = uow.Session.QueryOver<DeliveryPoint>()
				.Where(dp => dp.Counterparty.Id == counterpartyId)
				.And(dp => dp.IsActive)
				.SelectList(list => list
					.Select(dp => dp.Id).WithAlias(() => resultAlias.Id)
					.Select(dp => dp.Counterparty.Id).WithAlias(() => resultAlias.CounterpartyId)
					.Select(dp => dp.City).WithAlias(() => resultAlias.City)
					.Select(dp => dp.LocalityType).WithAlias(() => resultAlias.LocalityType)
					.Select(dp => dp.LocalityTypeShort).WithAlias(() => resultAlias.LocalityTypeShort)
					.Select(dp => dp.Street).WithAlias(() => resultAlias.Street)
					.Select(dp => dp.StreetType).WithAlias(() => resultAlias.StreetType)
					.Select(dp => dp.StreetTypeShort).WithAlias(() => resultAlias.StreetTypeShort)
					.Select(dp => dp.Building).WithAlias(() => resultAlias.Building)
					.Select(dp => dp.Floor).WithAlias(() => resultAlias.Floor)
					.Select(dp => dp.Entrance).WithAlias(() => resultAlias.Entrance)
					.Select(dp => dp.Room).WithAlias(() => resultAlias.Room)
					.Select(dp => dp.Latitude).WithAlias(() => resultAlias.Latitude)
					.Select(dp => dp.Longitude).WithAlias(() => resultAlias.Longitude)
					.Select(dp => dp.Category.Id).WithAlias(() => resultAlias.CategoryId)
					.Select(dp => dp.OnlineComment).WithAlias(() => resultAlias.OnlineComment)
					.Select(dp => dp.Intercom).WithAlias(() => resultAlias.Intercom)
				)
				.TransformUsing(Transformers.AliasToBean<DeliveryPointForSendNode>())
				.List<DeliveryPointForSendNode>();

			return result;
		}

		public bool ClientDeliveryPointExists(IUnitOfWork uow, int counterpartyId, int deliveryPointId)
		{
			var query = from deliveryPoint in uow.Session.Query<DeliveryPoint>()
				where deliveryPoint.Id == deliveryPointId && deliveryPoint.Counterparty.Id == counterpartyId
				select deliveryPoint.Id;

			return query.Any();
		}

		private string GetBuildingNumber(string building)
		{
			string buildingNumber = string.Empty;

			foreach(var ch in building)
			{
				if(char.IsDigit(ch))
				{
					buildingNumber += ch;
				}
				else
				{
					if(buildingNumber != string.Empty)
					{
						break;
					}
				}
			}

			return buildingNumber;
		}
	}
}
