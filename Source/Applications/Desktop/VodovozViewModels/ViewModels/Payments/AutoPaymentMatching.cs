using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MoreLinq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Documents;
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
		private readonly HashSet<int> _addedOrderIdsToAllocate = new HashSet<int>();

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

			var documentNumbers = ParsePaymentPurpose(payment.PaymentPurpose);

			if(documentNumbers.Any())
			{
				var numericOrderIds = documentNumbers
					.Where(d => int.TryParse(d, out _))
					.Select(int.Parse)
					.ToList();

				if(numericOrderIds.Any())
				{
					orders.AddRange(
						numericOrderIds.Select(orderId => _uow.GetById<Order>(orderId))
							.Where(order => order != null
								&& !_orderUndeliveredStatuses.Contains(order.OrderStatus)
								&& order.Client.Id == payment.Counterparty.Id
								&& order.PaymentType == PaymentType.Cashless
								&& (order.OrderPaymentStatus == OrderPaymentStatus.UnPaid || order.OrderPaymentStatus == OrderPaymentStatus.None)
								&& order.OrderSum > 0
								&& order.Contract.Organization.INN == payment.Organization.INN));
				}

				var formattedDocumentNumbers = documentNumbers
					.Where(d => !int.TryParse(d, out _))
					.ToList();

				if(formattedDocumentNumbers.Any())
				{
					var normalizedDocumentNumbers = formattedDocumentNumbers
						.Select(NormalizeDocumentNumber)
						.ToHashSet();

					var orderIdsByDocNumber = _uow.Session.Query<DocumentOrganizationCounter>()
						.Where(d => d.Order != null)
						.AsEnumerable()
						.Where(d => normalizedDocumentNumbers.Contains(NormalizeDocumentNumber(d.DocumentNumber)))
						.Select(d => d.Order.Id)
						.ToList();

					if(orderIdsByDocNumber.Any())
					{
						orders.AddRange(
							orderIdsByDocNumber.Select(orderId => _uow.GetById<Order>(orderId))
								.Where(order => order != null
									&& !_orderUndeliveredStatuses.Contains(order.OrderStatus)
									&& order.Client.Id == payment.Counterparty.Id
									&& order.PaymentType == PaymentType.Cashless
									&& (order.OrderPaymentStatus == OrderPaymentStatus.UnPaid || order.OrderPaymentStatus == OrderPaymentStatus.None)
									&& order.OrderSum > 0
									&& order.Contract.Organization.INN == payment.Organization.INN));
					}
				}

				orders = orders.DistinctBy(o => o.Id).ToList();

				if(!orders.Any())
				{
					return false;
				}

				var paymentSum = payment.Total;

				foreach(var order in orders)
				{
					if(_addedOrderIdsToAllocate.Contains(order.Id))
					{
						return false;
					}
					
					if(paymentSum >= order.OrderSum)
					{
						payment.AddPaymentItem(order);
						_addedOrderIdsToAllocate.Add(order.Id);
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

			payment.NumOrders = sb.ToString().TrimEnd('\r', '\n');
			return true;
		}

		private ISet<string> ParsePaymentPurpose(string paymentPurpose)
		{
			var pattern = @"([А-ЯA-Za-zа-я]{2,3}\d{2}-\d+|\d{6,7})";
			var uniqueDocumentNumbers = new HashSet<string>();
			var matches = Regex.Matches(paymentPurpose, pattern);

			for(var i = 0; i < matches.Count; i++)
			{
				uniqueDocumentNumbers.Add(matches[i].Groups[1].Value);
			}

			return uniqueDocumentNumbers;
		}

		private string NormalizeDocumentNumber(string docNumber)
		{
			var russianToLatin = new Dictionary<char, char>
			{
				{ 'А', 'A' }, { 'а', 'a' },
				{ 'В', 'B' }, { 'в', 'b' },
				{ 'Е', 'E' }, { 'е', 'e' },
				{ 'К', 'K' }, { 'к', 'k' },
				{ 'М', 'M' }, { 'м', 'm' },
				{ 'Н', 'H' }, { 'н', 'h' },
				{ 'О', 'O' }, { 'о', 'o' },
				{ 'Р', 'P' }, { 'р', 'p' },
				{ 'С', 'C' }, { 'с', 'c' },
				{ 'Т', 'T' }, { 'т', 't' },
				{ 'У', 'Y' }, { 'у', 'y' },
				{ 'Х', 'X' }, { 'х', 'x' }
			};

			var result = new StringBuilder();
			foreach(var c in docNumber)
			{
				result.Append(russianToLatin.ContainsKey(c) ? russianToLatin[c] : c);
			}
			return result.ToString().ToUpperInvariant();
		}
	}
}
