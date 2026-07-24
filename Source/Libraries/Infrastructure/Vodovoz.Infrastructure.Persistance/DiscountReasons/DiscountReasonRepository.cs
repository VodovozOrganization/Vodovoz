using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;

namespace Vodovoz.Infrastructure.Persistance.DiscountReasons
{
	internal sealed class DiscountReasonRepository : IDiscountReasonRepository
	{
		/// <inheritdoc/>
		public IList<DiscountReason> GetDiscountReasons(IUnitOfWork uow, bool orderByDescending = false)
		{
			var query = uow.Session.QueryOver<DiscountReason>()
				.OrderBy(i => i.Name);
			return orderByDescending ? query.Desc().List() : query.Asc().List();
		}
		
		/// <inheritdoc/>
		public IEnumerable<DiscountReason> GetDiscountReasons(IUnitOfWork uow, IEnumerable<int> disсountReasonIds)
		{
			var query = uow.Session.Query<DiscountReason>()
				.Where(x => disсountReasonIds.Contains(x.Id));
			
			return query.ToList();
		}

		public IList<DiscountReason> GetActiveDiscountReasons(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<DiscountReason>()
				.WhereNot(dr => dr.IsArchive)
				.OrderBy(dr => dr.Name)
				.Asc()
				.List();
		}

		public IList<DiscountReason> GetActiveDiscountReasonsWithoutPremiums(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<DiscountReason>()
				.Where(dr => !dr.IsArchive)
				.And(dr => !dr.IsPremiumDiscount)
				.OrderBy(dr => dr.Name)
				.Asc()
				.List();
		}

		public IList<DiscountReason> GetActiveDiscountReasonsFetchReferences(IUnitOfWork uow, bool canChoosePremiumDiscount)
		{
			var mainQuery = CreateBaseActiveDiscountReasonsQuery(uow, canChoosePremiumDiscount)
				.Future<DiscountReason>();

			CreateBaseActiveDiscountReasonsQuery(uow, canChoosePremiumDiscount)
				.Fetch(SelectMode.Fetch, dr => dr.Nomenclatures)
				.Future<DiscountReason>();

			CreateBaseActiveDiscountReasonsQuery(uow, canChoosePremiumDiscount)
				.Fetch(SelectMode.Fetch, dr => dr.NomenclatureCategories)
				.Future<DiscountReason>();

			CreateBaseActiveDiscountReasonsQuery(uow, canChoosePremiumDiscount)
				.Fetch(SelectMode.Fetch, dr => dr.ProductGroups)
				.Future<DiscountReason>();

			return mainQuery.ToList()
				.Distinct()
				.OrderBy(dr => dr.Name)
				.ToList();
		}

		private IQueryOver<DiscountReason, DiscountReason> CreateBaseActiveDiscountReasonsQuery(
			IUnitOfWork uow, bool canChoosePremiumDiscount)
		{
			var query = uow.Session.QueryOver<DiscountReason>()
				.Where(dr => !dr.IsArchive);

			if(!canChoosePremiumDiscount)
			{
				query = query.And(dr => !dr.IsPremiumDiscount);
			}

			return query;
		}

		public bool ExistsActiveDiscountReasonWithName(
			IUnitOfWork uow, int discountReasonId, string name, out DiscountReason discountReason)
		{
			discountReason = uow.Session.QueryOver<DiscountReason>()
				.Where(dr => !dr.IsArchive)
				.And(dr => dr.Id != discountReasonId)
				.And(dr => dr.Name == name)
				.SingleOrDefault();

			return discountReason != null;
		}

		public DiscountReason GetActivePromoCode(IUnitOfWork uow, string promoCode)
		{
			var discount = (
				from discountReason in uow.Session.Query<DiscountReason>()
				where discountReason.IsPromoCode
					&& !discountReason.IsArchive
					&& discountReason.PromoCodeName.ToLower() == promoCode.ToLower()
				select discountReason)
				.SingleOrDefault();
			
			return discount;
		}
		
		public bool ExistsPromoCodeWithName(IUnitOfWork uow, int discountReasonId, string promoCode, out DiscountReason discountReason)
		{
			discountReason = (
				from discount in uow.Session.Query<DiscountReason>()
				where discount.IsPromoCode
					&& discount.PromoCodeName.ToLower() == promoCode.ToLower()
					&& discount.Id != discountReasonId
				select discount)
				.SingleOrDefault();

			return discountReason != null;
		}

		public bool HasBeenUsagePromoCode(IUnitOfWork uow, int? counterpartyId, int discountReasonId)
		{
			if(!counterpartyId.HasValue)
			{
				return true;
			}

			var onlineOrderItems = 
				from onlineOrderItem in uow.Session.Query<OnlineOrderItem>()
				join onlineOrder in uow.Session.Query<OnlineOrder>()
					on onlineOrderItem.OnlineOrder.Id equals onlineOrder.Id
				where onlineOrder.Counterparty.Id == counterpartyId
				      && onlineOrderItem.DiscountReasons.Any(r => r.Id == discountReasonId)
					  && onlineOrder.OnlineOrderStatus != OnlineOrderStatus.Canceled
				select onlineOrderItem;

			if(onlineOrderItems.Any())
			{
				return true;
			}
			
			var orderItems = 
				from orderItem in uow.Session.Query<OrderItem>()
				join order in uow.Session.Query<Vodovoz.Domain.Orders.Order>()
					on orderItem.Order.Id equals order.Id
				where order.Client.Id == counterpartyId
					&& orderItem.DiscountReasons.Any(r => r.Id == discountReasonId)
					&& order.OrderStatus != OrderStatus.DeliveryCanceled
					&& order.OrderStatus != OrderStatus.Canceled
					&& order.OrderStatus != OrderStatus.NotDelivered
				select orderItem;

			return orderItems.Any();
		}
	}
}
