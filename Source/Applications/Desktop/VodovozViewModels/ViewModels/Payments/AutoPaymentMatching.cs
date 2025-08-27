using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class AutoPaymentMatching
	{
		private readonly IUnitOfWork _uow;
		private readonly OrderStatus[] _orderUndeliveredStatuses;
		private readonly HashSet<int> addedOrderIdsToAllocate = new HashSet<int>();

		public AutoPaymentMatching(IUnitOfWork uow, IOrderRepository orderRepository)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_orderUndeliveredStatuses = (orderRepository ?? throw new ArgumentNullException(nameof(orderRepository)))
				.GetUndeliveryStatuses();
		}

		public bool IncomePaymentMatch(Payment payment)
		{
			var sb = new StringBuilder();
			var orders = new List<Order>();

			if(payment.Counterparty == null)
			{
				return false;
			}

			var uniqueOrderNumbers = ParsePaymentPurpose(payment.PaymentPurpose);

			if(uniqueOrderNumbers.Any())
			{
				orders.AddRange(
					uniqueOrderNumbers.Select(orderNumber => _uow.GetById<Order>(orderNumber))
						.Where(order => order != null
							&& !_orderUndeliveredStatuses.Contains(order.OrderStatus)
							&& order.Client.Id == payment.Counterparty.Id
							&& order.PaymentType == PaymentType.Cashless
							&& (order.OrderPaymentStatus == OrderPaymentStatus.UnPaid || order.OrderPaymentStatus == OrderPaymentStatus.None)
							&& order.OrderSum > 0
							&& order.Contract.Organization.INN == payment.Organization.INN));

				if(!orders.Any())
				{
					return false;
				}

				var paymentSum = payment.Total;

				foreach(var order in orders)
				{
					if(addedOrderIdsToAllocate.Contains(order.Id))
					{
						return false;
					}
					
					if(paymentSum >= order.OrderSum)
					{
						payment.AddPaymentItem(order);
						addedOrderIdsToAllocate.Add(order.Id);
						sb.AppendLine(order.Id.ToString());
						paymentSum -= order.OrderSum;
					}

					if(paymentSum == 0)
					{
						break;
					}
				}
			}

			if(!payment.Items.Any())
			{
				return false;
			}

			payment.NumOrders = sb.ToString().TrimEnd(new[] { '\r', '\n' });
			return true;
		}

		private ISet<int> ParsePaymentPurpose(string paymentPurpose)
		{
			string pattern = @"([0-9]{6,7})";

			HashSet<int> uniqueOrderNumbers = new HashSet<int>();
			var matches = Regex.Matches(paymentPurpose, pattern);

			for(int i = 0; i < matches.Count; i++)
			{
				uniqueOrderNumbers.Add(int.Parse(matches[i].Groups[1].Value));
			}

			return uniqueOrderNumbers;
		}
	}
}
