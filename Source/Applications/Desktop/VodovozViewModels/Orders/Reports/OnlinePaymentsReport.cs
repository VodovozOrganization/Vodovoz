using QS.DomainModel.UoW;
using System;
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
	public class OnlinePaymentsReport
	{
		private OnlinePaymentsReport(
			DateTime startDate,
			DateTime endDate)
		{
			StartDate = startDate;
			EndDate = endDate;
		}

		public DateTime StartDate { get; }
		public DateTime EndDate { get; }

		public static Task<Result<OnlinePaymentsReport>> CreateAsync(
			DateTime startDate,
			DateTime endDate,
			IUnitOfWork unitOfWork,
			CancellationToken cancellationToken)
		{
			var startTime = DateTime.Now;

			var avangardPayments = new int[] { 10, 11, 12, 13 };

			if(cancellationToken.IsCancellationRequested)
			{
				return Task.FromResult(Result.Failure<OnlinePaymentsReport>(Report.CreateAborted));
			}

			var orders = (from order in unitOfWork.Session.Query<Order>()
						  join counterparty in unitOfWork.Session.Query<Counterparty>()
						  on order.Client.Id equals counterparty.Id
						  join deliveryPoint in unitOfWork.Session.Query<DeliveryPoint>()
						  on order.DeliveryPoint.Id equals deliveryPoint.Id
						  where order.PaymentType == PaymentType.PaidOnline
							  && order.OnlineOrder != null
							  && !avangardPayments.Contains(order.PaymentByCardFrom.Id)
							  && order.DeliveryDate >= startDate
							  && order.DeliveryDate <= endDate
						  let address = order.SelfDelivery ? "Самовывоз" : deliveryPoint.ShortAddress
						  let orderTotalSum = (from orderItem in unitOfWork.Session.Query<OrderItem>()
											   where orderItem.Order.Id == order.Id
											   select orderItem.ActualSum).Sum(x => x as decimal? == null ? 0m : x)
						  select new
						  {
							  order.DeliveryDate,
							  order.Id,
							  counterparty.FullName,
							  Address = address,
							  order.OnlineOrder,
							  TotalSumFromBank = 0,
							  OrderTotalSum = orderTotalSum,
							  order.OrderStatus,
							  Author = order.Author.ShortName,
							  PaymentDateTimeOrError = "Оплата не найдена",
							  order.PaymentType,
							  IsFutureOrder = order.DeliveryDate > endDate,
							  NumberAndShop = ""
						  }).ToList();

			var onlineOrdersIds = orders.Select(x => x.OnlineOrder);

			if(cancellationToken.IsCancellationRequested)
			{
				return Task.FromResult(Result.Failure<OnlinePaymentsReport>(Report.CreateAborted));
			}

			var payments = (from payment in unitOfWork.Session.Query<PaymentByCardOnline>()
							where payment.PaymentByCardFrom != PaymentByCardOnlineFrom.FromSMS
								 && onlineOrdersIds.Contains(payment.PaymentNr)
							select payment)
							.ToList();

			var ordersIds = orders.Select(x => x.Id);

			if(cancellationToken.IsCancellationRequested)
			{
				return Task.FromResult(Result.Failure<OnlinePaymentsReport>(Report.CreateAborted));
			}

			var paymentsBySms = (from payment in unitOfWork.Session.Query<PaymentByCardOnline>()
								 where payment.PaymentByCardFrom == PaymentByCardOnlineFrom.FromSMS
									  && ordersIds.Contains(payment.PaymentNr)
								 select payment)
								.ToList();

			var generatedInMilliseconds = (DateTime.Now - startTime).TotalMilliseconds;

			return Task.FromResult(Result.Success(new OnlinePaymentsReport(startDate, endDate)));
		}
	}
}
