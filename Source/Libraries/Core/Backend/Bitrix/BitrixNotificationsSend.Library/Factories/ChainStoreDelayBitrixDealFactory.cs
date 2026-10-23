using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Orders;
using VodovozBusiness.EntityRepositories.Nodes;

namespace BitrixNotificationsSend.Library.Factories
{
	/// <inheritdoc/>
	public class ChainStoreDelayBitrixDealFactory : IChainStoreDelayBitrixDealFactory
	{
		private const string _selfDeliveryAddress = "Самовывоз";

		/// <summary>
		/// Длина колонки chain_store_delay_bitrix_deals.deal_title
		/// </summary>
		private const int _dealTitleMaxLength = 255;

		/// <inheritdoc/>
		public IEnumerable<ChainStoreDelayBitrixDeal> CreateDeals(
			IEnumerable<ChainStoreDelayOrderNode> orders,
			ISet<int> previouslySentOrderIds,
			DateTime creationDate)
		{
			if(orders is null)
			{
				throw new ArgumentNullException(nameof(orders));
			}

			if(previouslySentOrderIds is null)
			{
				throw new ArgumentNullException(nameof(previouslySentOrderIds));
			}

			return orders
				.Where(x => !previouslySentOrderIds.Contains(x.OrderId))
				.OrderBy(x => x.EstimatedPaymentDate)
				.ThenBy(x => x.OrderId)
				.Select(x => CreateDeal(x, creationDate));
		}

		private static ChainStoreDelayBitrixDeal CreateDeal(ChainStoreDelayOrderNode order, DateTime creationDate)
		{
			return new ChainStoreDelayBitrixDeal
			{
				CreationDate = creationDate,
				Status = ChainStoreDelayBitrixDealStatus.DealCreateRequired,
				CounterpartyId = order.CounterpartyId,
				OrderId = order.OrderId,
				DealTitle = CreateDealTitle(order),
				CounterpartyName = order.CounterpartyName,
				CounterpartyInn = order.CounterpartyInn,
				PhoneNumber = order.ContactPhoneNumber,
				DeliveryAddress = order.IsSelfDelivery ? _selfDeliveryAddress : order.DeliveryPointShortAddress,
				OrderAuthor = order.AuthorShortName,
				OrderSum = order.OrderSum,
				Debt = order.Debt,
				EstimatedPaymentDate = order.EstimatedPaymentDate,
				DaysOverdue = 0
			};
		}

		/// <summary>
		/// Название сделки "Контрагент, заказ N"
		/// </summary>
		private static string CreateDealTitle(ChainStoreDelayOrderNode order)
		{
			var orderSuffix = $", заказ {order.OrderId}";
			var counterpartyName = order.CounterpartyName ?? string.Empty;
			var maxNameLength = Math.Max(0, _dealTitleMaxLength - orderSuffix.Length);

			return $"{counterpartyName.Substring(0, Math.Min(counterpartyName.Length, maxNameLength))}{orderSuffix}";
		}
	}
}
