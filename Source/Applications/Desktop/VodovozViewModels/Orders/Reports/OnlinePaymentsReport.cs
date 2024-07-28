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
			string selectedShop)
		{
			StartDate = startDate;
			EndDate = endDate;
			SelectedShop = selectedShop;
		}

		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public string SelectedShop { get; }

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

			var orders = await ordersQuery.ToListAsync();

			var onlineOrdersIds = orders.Select(x => x.OnlineOrderId);

			if(cancellationToken.IsCancellationRequested)
			{
				return await Task.FromResult(Result.Failure<OnlinePaymentsReport>(Report.CreateAborted));
			}

			IQueryable<PaymentByCardOnline> onlinePaymentsQuery = GetOnlinePaymentsQuery(unitOfWork, onlineOrdersIds);

			var onlinePayments = await onlinePaymentsQuery.ToListAsync();

			var ordersIds = orders.Select(x => x.OrderId);

			if(cancellationToken.IsCancellationRequested)
			{
				return await Task.FromResult(Result.Failure<OnlinePaymentsReport>(Report.CreateAborted));
			}

			IQueryable<PaymentByCardOnline> smsPymentsQuery = GetSmsPaymentsQuery(unitOfWork, ordersIds);

			var smsPayments = await smsPymentsQuery.ToListAsync();

			foreach(var payment in onlinePayments)
			{
				var orderRowToUpdate = orders.Where(x => x.OnlineOrderId == payment.PaymentNr).FirstOrDefault();

				if(orderRowToUpdate != null)
				{
					
				}
			}

			foreach(var payment in smsPayments)
			{
				var orderRowToUpdate = orders.Where(x => x.OrderId == payment.PaymentNr).FirstOrDefault();

				if(orderRowToUpdate != null)
				{

				}
			}

			var generatedInMilliseconds = (DateTime.Now - startTime).TotalMilliseconds;

			return await Task.FromResult(
				Result.Success(
					new OnlinePaymentsReport(
						startDate,
						endDate,
						selectedShop)));
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
				&& !_avangardPayments.Contains(order.PaymentByCardFrom.Id)
				&& order.DeliveryDate >= startDate
				&& order.DeliveryDate <= endDate
			let address = order.SelfDelivery ? "Самовывоз" : deliveryPoint.ShortAddress
			let orderTotalSum = (from orderItem in unitOfWork.Session.Query<OrderItem>()
								 where orderItem.Order.Id == order.Id
								 select orderItem.ActualSum).Sum(x => x as decimal? == null ? 0m : x)
			select new Row
			{
				DeliveryDate = order.DeliveryDate,
				OrderId = order.Id,
				CcounterpartyFullName = counterparty.FullName,
				Address = address,
				OnlineOrderId = order.OnlineOrder,
				TotalSumFromBank = 0,
				OrderTotalSum = orderTotalSum,
				OrderStatus = order.OrderStatus,
				Author = order.Author.ShortName,
				PaymentDateTimeOrError = "Оплата не найдена",
				OrderPaymentType = order.PaymentType,
				IsFutureOrder = order.DeliveryDate > endDate,
				NumberAndShop = ""
			};
	}
}
