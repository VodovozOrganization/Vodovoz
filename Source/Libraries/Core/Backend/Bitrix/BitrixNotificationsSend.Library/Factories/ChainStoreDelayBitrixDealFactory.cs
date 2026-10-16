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
		private const string _valuesSeparator = ", ";

		/// <inheritdoc/>
		public IEnumerable<ChainStoreDelayBitrixDeal> CreateDeals(
			IEnumerable<ChainStoreDelayOrderNode> orders,
			ISet<int> previouslySentOrderIds,
			ISet<int> counterpartyIdsWithTodayDeals,
			DateTime today,
			int daysBeforePaymentDate,
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

			if(counterpartyIdsWithTodayDeals is null)
			{
				throw new ArgumentNullException(nameof(counterpartyIdsWithTodayDeals));
			}

			var maxDate = today.Date.AddDays(daysBeforePaymentDate);

			var groups = orders
				.Where(x => x.EstimatedPaymentDate <= maxDate)
				.GroupBy(x => x.CounterpartyId);

			foreach(var group in groups)
			{
				if(counterpartyIdsWithTodayDeals.Contains(group.Key))
				{
					continue;
				}

				var groupOrders = group
					.OrderBy(x => x.EstimatedPaymentDate)
					.ThenBy(x => x.OrderId)
					.ToList();

				var initiator = groupOrders.FirstOrDefault(x =>
					x.EstimatedPaymentDate > today.Date
					&& x.EstimatedPaymentDate <= maxDate
					&& !previouslySentOrderIds.Contains(x.OrderId));

				if(initiator is null)
				{
					continue;
				}

				yield return CreateDeal(groupOrders, initiator, today.Date, creationDate);
			}
		}

		private static ChainStoreDelayBitrixDeal CreateDeal(
			IReadOnlyList<ChainStoreDelayOrderNode> groupOrders,
			ChainStoreDelayOrderNode initiator,
			DateTime today,
			DateTime creationDate)
		{
			var firstOrder = groupOrders[0];

			var deal = new ChainStoreDelayBitrixDeal
			{
				CreationDate = creationDate,
				Status = BitrixDealSynchronizationStatus.DealCreateRequired,
				CounterpartyId = firstOrder.CounterpartyId,
				InitiatorOrderId = initiator.OrderId,
				DealTitle = $"{firstOrder.CounterpartyName}, заказ {initiator.OrderId}",
				CounterpartyName = firstOrder.CounterpartyName,
				CounterpartyInn = firstOrder.CounterpartyInn,
				PhoneNumber = initiator.ContactPhoneNumber,
				DeliveryAddresses = JoinDistinct(groupOrders.Select(x =>
					x.IsSelfDelivery ? _selfDeliveryAddress : x.DeliveryPointShortAddress)),
				OrderAuthors = JoinDistinct(groupOrders.Select(x => x.AuthorShortName)),
				OrdersSum = groupOrders.Sum(x => x.OrderSum),
				DebtSum = groupOrders.Sum(x => x.Debt),
				InitiatorEstimatedPaymentDate = initiator.EstimatedPaymentDate,
				DaysOverdue = (today - firstOrder.EstimatedPaymentDate).Days
			};

			foreach(var order in groupOrders)
			{
				deal.Orders.Add(new ChainStoreDelayBitrixDealOrder
				{
					Deal = deal,
					OrderId = order.OrderId,
					DeliveryPointId = order.DeliveryPointId,
					IsSelfDelivery = order.IsSelfDelivery,
					EstimatedPaymentDate = order.EstimatedPaymentDate,
					OrderSum = order.OrderSum,
					Debt = order.Debt
				});
			}

			return deal;
		}

		private static string JoinDistinct(IEnumerable<string> values)
		{
			return string.Join(_valuesSeparator, values.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct());
		}
	}
}
