using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Infrastructure.Persistance.Orders;

namespace Vodovoz.Infrastructure.Persistance.DiscountReasons
{
	internal sealed class DiscountReasonRepository : IDiscountReasonRepository
	{
		/// <summary>
		/// Возврат отсортированного списка скидок
		/// </summary>
		/// <returns>Список скидок</returns>
		/// <param name="UoW">UoW</param>
		/// <param name="orderByDescending">Если <c>true</c>, то сортируется список по убыванию имени скидки</param>
		public IList<DiscountReason> GetDiscountReasons(IUnitOfWork UoW, bool orderByDescending = false)
		{
			var query = UoW.Session.QueryOver<DiscountReason>()
				.OrderBy(i => i.Name);
			return orderByDescending ? query.Desc().List() : query.Asc().List();
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
				where discountReason.IsPromoCode && discountReason.PromoCodeName.ToLower() == promoCode.ToLower()
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

		public bool HasBeenUsagePromoCode(IUnitOfWork uow, int counterpartyId, int discountReasonId)
		{
			var onlineOrderItems = 
				from onlineOrderItem in uow.Session.Query<OnlineOrderItem>()
				join onlineOrder in uow.Session.Query<OnlineOrder>()
					on onlineOrderItem.OnlineOrder.Id equals onlineOrder.Id
				where onlineOrder.Counterparty.Id == counterpartyId
				      && onlineOrderItem.DiscountReason.Id == discountReasonId
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
					&& orderItem.DiscountReason.Id == discountReasonId
					&& order.OrderStatus != OrderStatus.DeliveryCanceled
					&& order.OrderStatus != OrderStatus.Canceled
					&& order.OrderStatus != OrderStatus.NotDelivered
				select orderItem;

			return orderItems.Any();
		}
	}
}
