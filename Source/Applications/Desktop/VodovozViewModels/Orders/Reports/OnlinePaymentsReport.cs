using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;
using Vodovoz.Errors;
using Vodovoz.Presentation.ViewModels.Errors;

namespace Vodovoz.ViewModels.Orders.Reports
{
	public partial class OnlinePaymentsReport
	{
		private static readonly int[] _avangardPayments = new int[] { 10, 11, 12, 13 };

		private OnlinePaymentsReport(
			DateTime startDate,
			DateTime endDate,
			string selectedShop,
			IEnumerable<Row> paidOrders,
			IEnumerable<Row> paymentMissingOrders,
			IEnumerable<Row> overpaidOrders,
			IEnumerable<Row> underpaidOrders)
		{
			StartDate = startDate;
			EndDate = endDate;
			SelectedShop = selectedShop;
			PaidOrders = paidOrders;
			PaymentMissingOrders = paymentMissingOrders;
			OverpaidOrders = overpaidOrders;
			UnderpaidOrders = underpaidOrders;
		}

		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public string SelectedShop { get; }
		public IEnumerable<Row> PaidOrders { get; }
		public IEnumerable<Row> PaymentMissingOrders { get; }
		public IEnumerable<Row> OverpaidOrders { get; }
		public IEnumerable<Row> UnderpaidOrders { get; }

		public static async Task<Result<OnlinePaymentsReport>> CreateAsync(
			DateTime startDate,
			DateTime endDate,
			string selectedShop,
			IUnitOfWork unitOfWork,
			CancellationToken cancellationToken)
		{
			var startTime = DateTime.Now;

			if(cancellationToken.IsCancellationRequested)
			{
				return await Task.FromResult(Result.Failure<OnlinePaymentsReport>(Report.CreateAborted));
			}

			IQueryable<Row> ordersQuery = GetOrdersQuery(startDate, endDate, unitOfWork);

			var orders = await ordersQuery.ToListAsync(cancellationToken);

			var onlineOrdersIds = orders.Select(x => x.OnlineOrderId);

			if(cancellationToken.IsCancellationRequested)
			{
				return await Task.FromResult(Result.Failure<OnlinePaymentsReport>(Report.CreateAborted));
			}

			IQueryable<PaymentByCardOnline> onlinePaymentsQuery = GetOnlinePaymentsQuery(unitOfWork, onlineOrdersIds);

			var onlinePayments = await onlinePaymentsQuery.ToListAsync(cancellationToken);

			var ordersIds = orders.Select(x => x.OrderId);

			if(cancellationToken.IsCancellationRequested)
			{
				return await Task.FromResult(Result.Failure<OnlinePaymentsReport>(Report.CreateAborted));
			}

			IQueryable<PaymentByCardOnline> smsPymentsQuery = GetSmsPaymentsQuery(unitOfWork, ordersIds);

			var smsPayments = await smsPymentsQuery.ToListAsync(cancellationToken);

			foreach(var payment in onlinePayments)
			{
				var orderRowToUpdate = orders.Where(x => x.OnlineOrderId == payment.PaymentNr).FirstOrDefault();

				if(orderRowToUpdate != null)
				{
					if(payment.DateAndTime < orderRowToUpdate.OrderCreateDate.Value.AddDays(-45)
						|| payment.DateAndTime > orderRowToUpdate.OrderCreateDate.Value.AddDays(45))
					{
						continue;
					}

					UpdatePaymentInfo(payment, orderRowToUpdate);
				}
			}

			foreach(var payment in smsPayments)
			{
				var orderRowToUpdate = orders.Where(x => x.OrderId == payment.PaymentNr).FirstOrDefault();

				if(orderRowToUpdate != null)
				{
					if(payment.DateAndTime < orderRowToUpdate.OrderCreateDate.Value.AddDays(-45)
						|| payment.DateAndTime > orderRowToUpdate.OrderCreateDate.Value.AddDays(45))
					{
						continue;
					}

					UpdatePaymentInfo(payment, orderRowToUpdate);
				}
			}

			var paidOrders = orders
				.Where(or => or.ReportPaymentStatusEnum == Row.ReportPaymentStatus.Paid)
				.OrderByDescending(or => or.OrderStatusOrderingValue)
				.ToList();

			var paymentMissingOrders = orders
				.Where(or => or.ReportPaymentStatusEnum == Row.ReportPaymentStatus.Missing)
				.OrderByDescending(or => or.OrderStatusOrderingValue)
				.ToList();

			var overpaidOrders = orders
				.Where(or => or.ReportPaymentStatusEnum == Row.ReportPaymentStatus.OverPaid)
				.OrderByDescending(or => or.OrderStatusOrderingValue)
				.ToList();

			var underpaidOrders = orders
				.Where(or => or.ReportPaymentStatusEnum == Row.ReportPaymentStatus.UnderPaid)
				.OrderByDescending(or => or.OrderStatusOrderingValue)
				.ToList();

			var generatedInMilliseconds = (DateTime.Now - startTime).TotalMilliseconds;

			return await Task.FromResult(
				Result.Success(
					new OnlinePaymentsReport(
						startDate,
						endDate,
						selectedShop,
						paidOrders,
						paymentMissingOrders,
						overpaidOrders,
						underpaidOrders)));
		}

		private static void UpdatePaymentInfo(PaymentByCardOnline payment, Row orderRowToUpdate)
		{
			orderRowToUpdate.PaymentId = payment.Id;
			orderRowToUpdate.PaymentDateTimeOrError = $"{payment.DateAndTime:dd.MM.yyyy hh:mm:ss}";
			orderRowToUpdate.TotalSumFromBank = payment.PaymentRUR;

			if(orderRowToUpdate.TotalSumFromBank == orderRowToUpdate.OrderTotalSum)
			{
				orderRowToUpdate.ReportPaymentStatusEnum = Row.ReportPaymentStatus.Paid;
			}

			if(orderRowToUpdate.TotalSumFromBank > orderRowToUpdate.OrderTotalSum)
			{
				orderRowToUpdate.ReportPaymentStatusEnum = Row.ReportPaymentStatus.OverPaid;
			}

			if(orderRowToUpdate.TotalSumFromBank < orderRowToUpdate.OrderTotalSum)
			{
				orderRowToUpdate.ReportPaymentStatusEnum = Row.ReportPaymentStatus.UnderPaid;
			}

			if(!string.IsNullOrWhiteSpace(payment.Shop))
			{
				orderRowToUpdate.NumberAndShop = payment.PaymentNr + " \n" + payment.Shop;
			}
		}

		private static IQueryable<PaymentByCardOnline> GetSmsPaymentsQuery(IUnitOfWork unitOfWork, IEnumerable<int> ordersIds) =>
			from payment in unitOfWork.Session.Query<PaymentByCardOnline>()
			where payment.PaymentByCardFrom == PaymentByCardOnlineFrom.FromSMS
				 && ordersIds.Contains(payment.PaymentNr)
			select payment;

		private static IQueryable<PaymentByCardOnline> GetOnlinePaymentsQuery(IUnitOfWork unitOfWork, IEnumerable<int?> onlineOrdersIds) =>
			from payment in unitOfWork.Session.Query<PaymentByCardOnline>()
			where payment.PaymentByCardFrom != PaymentByCardOnlineFrom.FromSMS
				 && onlineOrdersIds.Contains(payment.PaymentNr)
			select payment;

		private static IQueryable<Row> GetOrdersQuery(DateTime startDate, DateTime endDate, IUnitOfWork unitOfWork) =>
			from order in unitOfWork.Session.Query<Order>()
			join counterparty in unitOfWork.Session.Query<Counterparty>()
			on order.Client.Id equals counterparty.Id
			join deliveryPoint in unitOfWork.Session.Query<DeliveryPoint>()
			on order.DeliveryPoint.Id equals deliveryPoint.Id
			where order.PaymentType == PaymentType.PaidOnline
				&& order.OnlineOrder != null
				&& (order.PaymentByCardFrom == null || !_avangardPayments.Contains(order.PaymentByCardFrom.Id))
				&& order.DeliveryDate >= startDate
				&& order.DeliveryDate <= endDate
			let address = order.SelfDelivery ? "Самовывоз" : deliveryPoint.ShortAddress
			let orderTotalSum = (decimal?)(from orderItem in unitOfWork.Session.Query<OrderItem>()
										   where orderItem.Order.Id == order.Id
										   select orderItem.ActualSum).Sum() ?? 0m
			select new Row
			{
				OrderCreateDate = order.CreateDate,
				OrderDeliveryDate = order.DeliveryDate,
				OrderId = order.Id,
				CcounterpartyFullName = counterparty.FullName,
				Address = address,
				OnlineOrderId = order.OnlineOrder,
				TotalSumFromBank = 0,
				OrderTotalSum = orderTotalSum,
				OrderStatus = order.OrderStatus,
				Author = order.Author.ShortName,
				PaymentId = null,
				PaymentDateTimeOrError = "Оплата не найдена",
				ReportPaymentStatusEnum = Row.ReportPaymentStatus.Missing,
				OrderPaymentType = order.PaymentType,
				IsFutureOrder = order.DeliveryDate > endDate,
				NumberAndShop = order.OnlineOrder.ToString()
			};
	}
}
