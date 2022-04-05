using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.FastPayments;

namespace Vodovoz.EntityRepositories.FastPayments
{
	public class FastPaymentRepository : IFastPaymentRepository
	{
		public IList<FastPayment> GetAllPerformedOrProcessingFastPaymentsByOrder(IUnitOfWork uow, int orderId)
		{
			return uow.Session.QueryOver<FastPayment>()
				.Where(fp => fp.Order.Id == orderId)
				.And(fp => fp.FastPaymentStatus == FastPaymentStatus.Performed || fp.FastPaymentStatus == FastPaymentStatus.Processing)
				.OrderBy(fp => fp.FastPaymentStatus).Desc
				.List();
		}

		public FastPaymentStatus? GetOrderFastPaymentStatus(IUnitOfWork uow, int orderId)
		{
			var fastPayments = GetAllFastPaymentsByOrderOrderByStatusDesc(uow, orderId);

			if(fastPayments.Any())
			{
				return fastPayments[0].FastPaymentStatus;
			}

			return null;
		}

		public FastPayment GetFastPaymentByTicket(IUnitOfWork uow, string ticket)
		{
			return GetFastPaymentByTicketQuery(ticket)
				.GetExecutableQueryOver(uow.Session)
				.SingleOrDefault();
		}
		
		public bool FastPaymentWithTicketExists(IUnitOfWork uow, string ticket)
		{
			var fastPayment = GetFastPaymentByTicket(uow, ticket);
			return fastPayment != null;
		}
		
		public IEnumerable<FastPayment> GetAllProcessingFastPayments(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<FastPayment>()
				.Where(fp => fp.FastPaymentStatus == FastPaymentStatus.Processing)
				.List();
		}
		
		public FastPayment GetProcessingPaymentForOrder(IUnitOfWork uow, int orderId)
		{
			return uow.Session.QueryOver<FastPayment>()
				.Where(fp => fp.Order.Id == orderId)
				.And(fp => fp.FastPaymentStatus == FastPaymentStatus.Processing)
				.SingleOrDefault();
		}
		
		private IList<FastPayment> GetAllFastPaymentsByOrderOrderByStatusDesc(IUnitOfWork uow, int orderId)
		{
			return uow.Session.QueryOver<FastPayment>()
				.Where(fp => fp.Order.Id == orderId)
				.OrderBy(fp => fp.FastPaymentStatus).Desc
				.List();
		}

		private QueryOver<FastPayment, FastPayment> GetFastPaymentByTicketQuery(string ticket)
		{
			return QueryOver.Of<FastPayment>()
				.Where(fp => fp.Ticket == ticket);
		}
	}
}
