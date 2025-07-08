using NHibernate.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;
using Vodovoz.Presentation.ViewModels.Errors;
using Vodovoz.Presentation.ViewModels.Reports;

namespace Vodovoz.ViewModels.Orders.Reports
{
	[Appellative(Nominative = "Отчет по оплатам OnLine заказов")]
	public partial class OnlinePaymentsReport : IClosedXmlReport
	{
		private static readonly int[] _avangardPayments = new int[] { 10, 11, 12, 13 };

		private OnlinePaymentsReport(
			DateTime startDate,
			DateTime endDate,
			string selectedShop,
			IEnumerable<OrderRow> paidOrders,
			IEnumerable<OrderRow> futurePaidOrders,
			IEnumerable<OrderRow> paymentMissingOrders,
			IEnumerable<OrderRow> futurePaymentMissingOrders,
			IEnumerable<OrderRow> overpaidOrders,
			IEnumerable<OrderRow> futureOverpaidOrders,
			IEnumerable<OrderRow> underpaidOrders,
			IEnumerable<OrderRow> futureUnderpaidOrders,
			IEnumerable<PaymentWithoutOrderRow> paymentsWithoutOrders,
			DateTime createdAt)
		{
			StartDate = startDate;
			EndDate = endDate;
			Shop = selectedShop;
			PaidOrders = paidOrders;
			FuturePaidOrders = futurePaidOrders;
			PaymentMissingOrders = paymentMissingOrders;
			FuturePaymentMissingOrders = futurePaymentMissingOrders;
			OverpaidOrders = overpaidOrders;
			FutureOverpaidOrders = futureOverpaidOrders;
			UnderpaidOrders = underpaidOrders;
			FutureUnderpaidOrders = futureUnderpaidOrders;
			PaymentsWithoutOrders = paymentsWithoutOrders;

			CreatedAt = createdAt;
		}

		public string TemplatePath => @".\Reports\Payments\OnlinePaymentsReportFromTBank.xlsx";
		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public string Shop { get; }
		public IEnumerable<OrderRow> PaidOrders { get; }
		public IEnumerable<OrderRow> PaymentMissingOrders { get; }
		public IEnumerable<OrderRow> OverpaidOrders { get; }
		public IEnumerable<OrderRow> UnderpaidOrders { get; }

		public IEnumerable<OrderRow> FuturePaidOrders { get; internal set; }
		public IEnumerable<OrderRow> FuturePaymentMissingOrders { get; internal set; }
		public IEnumerable<OrderRow> FutureOverpaidOrders { get; internal set; }
		public IEnumerable<OrderRow> FutureUnderpaidOrders { get; internal set; }
		public IEnumerable<PaymentWithoutOrderRow> PaymentsWithoutOrders { get; }
		public DateTime CreatedAt { get; }

		public string StartDateString => StartDate.ToString("dd.MM.yyyy");
		public string EndDateString => EndDate.ToString("dd.MM.yyyy");

		public string CreatedAtString => CreatedAt.ToString("dd.MM.yyyy hh:mm:ss");

		#region Generation

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

			IQueryable<OrderRow> ordersQuery = GetOrdersQuery(startDate, endDate, unitOfWork);

			var orders = await ordersQuery.ToListAsync(cancellationToken);

			var onlineOrdersIds = orders.Select(x => x.OnlineOrderId);

			if(cancellationToken.IsCancellationRequested)
			{
				return await Task.FromResult(Result.Failure<OnlinePaymentsReport>(Report.CreateAborted));
			}

			IQueryable<PaymentByCardOnline> onlinePaymentsQuery = GetOnlinePaymentsQuery(unitOfWork, onlineOrdersIds, selectedShop);

			var onlinePayments = await onlinePaymentsQuery.ToListAsync(cancellationToken);

			var ordersIds = orders.Select(x => x.OrderId);

			if(cancellationToken.IsCancellationRequested)
			{
				return await Task.FromResult(Result.Failure<OnlinePaymentsReport>(Report.CreateAborted));
			}

			IQueryable<PaymentByCardOnline> smsPymentsQuery = GetSmsPaymentsQuery(unitOfWork, ordersIds, selectedShop);

			var smsPayments = await smsPymentsQuery.ToListAsync(cancellationToken);

			foreach(var payment in onlinePayments)
			{
				var orderRowsToUpdate = orders
					.Where(x => x.OnlineOrderId == payment.PaymentNr);

				var listOrdersToDelete = new List<int>();

				foreach(var orderRowToUpdate in orderRowsToUpdate)
				{
					if(orderRowToUpdate != null)
					{
						if(payment.DateAndTime < orderRowToUpdate.OrderCreateDate.Value.AddDays(-45)
							|| payment.DateAndTime > orderRowToUpdate.OrderCreateDate.Value.AddDays(45))
						{
							continue;
						}

						if(selectedShop == null
							|| payment.Shop == selectedShop
							|| payment.Shop == null)
						{
							UpdatePaymentInfo(payment, orderRowToUpdate);
						}
						else
						{
							listOrdersToDelete.Add(orderRowToUpdate.OrderId);
						}
					}
				}

				orders.RemoveAll(x => listOrdersToDelete.Contains(x.OrderId));
			}

			foreach(var payment in smsPayments)
			{
				var orderRowsToUpdate = orders
					.Where(x => x.OrderId == payment.PaymentNr);

				var listOrdersToDelete = new List<int>();

				foreach(var orderRowToUpdate in orderRowsToUpdate)
				{
					if(orderRowToUpdate != null)
					{
						if(payment.DateAndTime < orderRowToUpdate.OrderCreateDate.Value.AddDays(-45)
							|| payment.DateAndTime > orderRowToUpdate.OrderCreateDate.Value.AddDays(45))
						{
							continue;
						}

						if(selectedShop == null
							|| payment.Shop == selectedShop
							|| payment.Shop == null)
						{
							UpdatePaymentInfo(payment, orderRowToUpdate);
						}
						else
						{
							listOrdersToDelete.Add(orderRowToUpdate.OrderId);
						}
					}
				}

				orders.RemoveAll(x => listOrdersToDelete.Contains(x.OrderId));
			}

			var paidOrders = orders
				.Where(or => or.ReportPaymentStatusEnum == OrderRow.ReportPaymentStatus.Paid)
				.OrderByDescending(or => or.OrderStatusOrderingValue)
				.ToList();

			var paymentMissingOrders = orders
				.Where(or => or.ReportPaymentStatusEnum == OrderRow.ReportPaymentStatus.Missing)
				.OrderByDescending(or => or.OrderStatusOrderingValue)
				.ToList();

			var overpaidOrders = orders
				.Where(or => or.ReportPaymentStatusEnum == OrderRow.ReportPaymentStatus.OverPaid)
				.OrderByDescending(or => or.OrderStatusOrderingValue)
				.ToList();

			var underpaidOrders = orders
				.Where(or => or.ReportPaymentStatusEnum == OrderRow.ReportPaymentStatus.UnderPaid)
				.OrderByDescending(or => or.OrderStatusOrderingValue)
				.ToList();

			if(cancellationToken.IsCancellationRequested)
			{
				return await Task.FromResult(Result.Failure<OnlinePaymentsReport>(Report.CreateAborted));
			}

			IQueryable<PaymentWithoutOrderRow> paymentsWithoutOrdersQuery = GetPaymentsWithoutOrdersQuery(startDate, endDate, selectedShop, unitOfWork);

			var paymentsWithoutOrders = await paymentsWithoutOrdersQuery.ToListAsync(cancellationToken);

			var generatedInMilliseconds = (DateTime.Now - startTime).TotalMilliseconds;

			var paidTodayOrders = paidOrders
				.Where(po => !po.IsFutureOrder)
				.ToList();

			var paidFutureOrders = paidOrders
				.Where(po => po.IsFutureOrder)
				.ToList();

			var paymentMissingTodayOrders = paymentMissingOrders
				.Where(po => !po.IsFutureOrder)
				.ToList();

			var paymentMissingFutureOrders = paymentMissingOrders
				.Where(po => po.IsFutureOrder)
				.ToList();

			var overpaidTodayOrders = overpaidOrders
				.Where(po => !po.IsFutureOrder)
				.ToList();

			var overpaidFutureOrders = overpaidOrders
				.Where(po => po.IsFutureOrder)
				.ToList();

			var underpaidTodayOrders = underpaidOrders
				.Where(po => !po.IsFutureOrder)
				.ToList();

			var underpaidFutureOrders = underpaidOrders
				.Where(po => po.IsFutureOrder)
				.ToList();

			if(!(paidTodayOrders.Any()
				|| paidFutureOrders.Any()
				|| paymentMissingTodayOrders.Any()
				|| paymentMissingFutureOrders.Any()
				|| overpaidTodayOrders.Any()
				|| overpaidFutureOrders.Any()
				|| underpaidTodayOrders.Any()
				|| underpaidFutureOrders.Any()
				|| paymentsWithoutOrders.Any()))
			{
				return await Task.FromResult(Result.Failure<OnlinePaymentsReport>(Report.NoData));
			}

			return await Task.FromResult(
				Result.Success(
					new OnlinePaymentsReport(
						startDate,
						endDate,
						selectedShop,
						paidTodayOrders,
						paidFutureOrders,
						paymentMissingTodayOrders,
						paymentMissingFutureOrders,
						overpaidTodayOrders,
						overpaidFutureOrders,
						underpaidTodayOrders,
						underpaidFutureOrders,
						paymentsWithoutOrders,
						startTime)));
		}

		private static IQueryable<PaymentWithoutOrderRow> GetPaymentsWithoutOrdersQuery(
			DateTime startDate,
			DateTime endDate,
			string selectedShop,
			IUnitOfWork unitOfWork) =>
			from payment in unitOfWork.Session.Query<PaymentByCardOnline>()
			where payment.DateAndTime >= startDate
				&& payment.DateAndTime <= endDate
				&& (selectedShop == null
					|| payment.Shop == selectedShop
					|| payment.Shop == null)
				&& !(from order in unitOfWork.Session.Query<Order>()
					 where (order.PaymentByCardFrom == null
							|| !_avangardPayments.Contains(order.PaymentByCardFrom.Id))
						&& payment.PaymentNr == order.OnlinePaymentNumber
						&& payment.PaymentByCardFrom != PaymentByCardOnlineFrom.FromSMS
					 select order.Id).Any()
				&& !(from order in unitOfWork.Session.Query<Order>()
					 where (order.PaymentByCardFrom == null
							|| !_avangardPayments.Contains(order.PaymentByCardFrom.Id))
						&& payment.PaymentNr == order.Id
						&& payment.PaymentByCardFrom == PaymentByCardOnlineFrom.FromSMS
					 select order.Id).Any()
			select new PaymentWithoutOrderRow
			{
				DateTime = payment.DateAndTime,
				Number = payment.PaymentNr,
				Shop = payment.Shop,
				Sum = payment.PaymentRUR,
				Email = payment.Email,
				Phone = payment.Phone,
				CounterpartyFullName = "Нет"
			};

		private static void UpdatePaymentInfo(PaymentByCardOnline payment, OrderRow orderRowToUpdate)
		{
			orderRowToUpdate.PaymentId = payment.Id;
			orderRowToUpdate.PaymentDateTimeOrError = $"{payment.DateAndTime:dd.MM.yyyy hh:mm:ss}";
			orderRowToUpdate.TotalSumFromBank = payment.PaymentRUR;

			if(orderRowToUpdate.TotalSumFromBank == orderRowToUpdate.OrderTotalSum)
			{
				orderRowToUpdate.ReportPaymentStatusEnum = OrderRow.ReportPaymentStatus.Paid;
			}

			if(orderRowToUpdate.TotalSumFromBank > orderRowToUpdate.OrderTotalSum)
			{
				orderRowToUpdate.ReportPaymentStatusEnum = OrderRow.ReportPaymentStatus.OverPaid;
			}

			if(orderRowToUpdate.TotalSumFromBank < orderRowToUpdate.OrderTotalSum)
			{
				orderRowToUpdate.ReportPaymentStatusEnum = OrderRow.ReportPaymentStatus.UnderPaid;
			}

			if(!string.IsNullOrWhiteSpace(payment.Shop))
			{
				orderRowToUpdate.NumberAndShop = payment.PaymentNr + " \n" + payment.Shop;
			}
		}

		private static IQueryable<PaymentByCardOnline> GetSmsPaymentsQuery(
			IUnitOfWork unitOfWork,
			IEnumerable<int> ordersIds,
			string selectedShop) =>
			from payment in unitOfWork.Session.Query<PaymentByCardOnline>()
			where payment.PaymentByCardFrom == PaymentByCardOnlineFrom.FromSMS
				 && ordersIds.Contains(payment.PaymentNr)
			select payment;

		private static IQueryable<PaymentByCardOnline> GetOnlinePaymentsQuery(
			IUnitOfWork unitOfWork,
			IEnumerable<int?> onlineOrdersIds,
			string selectedShop) =>
			from payment in unitOfWork.Session.Query<PaymentByCardOnline>()
			where payment.PaymentByCardFrom != PaymentByCardOnlineFrom.FromSMS
				 && onlineOrdersIds.Contains(payment.PaymentNr)
			select payment;

		private static IQueryable<OrderRow> GetOrdersQuery(
			DateTime startDate,
			DateTime endDate,
			IUnitOfWork unitOfWork) =>
			from order in unitOfWork.Session.Query<Order>()
			join counterparty in unitOfWork.Session.Query<Counterparty>()
			on order.Client.Id equals counterparty.Id
			where order.PaymentType == PaymentType.PaidOnline
				&& order.OnlinePaymentNumber != null
				&& (order.PaymentByCardFrom == null
					|| !_avangardPayments.Contains(order.PaymentByCardFrom.Id))
				&& order.DeliveryDate >= startDate
				&& order.DeliveryDate <= endDate
			let address = order.SelfDelivery
				? "Самовывоз"
				: (from deliveryPoint in unitOfWork.Session.Query<DeliveryPoint>()
				   where deliveryPoint.Id == order.DeliveryPoint.Id
				   select deliveryPoint.ShortAddress).FirstOrDefault()
			let orderTotalSum =
				(decimal?)(from orderItem in unitOfWork.Session.Query<OrderItem>()
						   where orderItem.Order.Id == order.Id
						   select orderItem.ActualSum).Sum() ?? 0m
			select new OrderRow
			{
				OrderCreateDate = order.CreateDate,
				OrderDeliveryDate = order.DeliveryDate,
				OrderId = order.Id,
				CcounterpartyFullName = counterparty.FullName,
				Address = address,
				OnlineOrderId = order.OnlinePaymentNumber,
				TotalSumFromBank = 0,
				OrderTotalSum = orderTotalSum,
				OrderStatus = order.OrderStatus,
				Author = order.Author.ShortName,
				PaymentId = null,
				PaymentDateTimeOrError = "Оплата не найдена",
				ReportPaymentStatusEnum = OrderRow.ReportPaymentStatus.Missing,
				OrderPaymentType = order.PaymentType,
				IsFutureOrder = order.DeliveryDate > endDate,
				NumberAndShop = order.OnlinePaymentNumber.ToString()
			};

		#endregion Generation
	}
}
