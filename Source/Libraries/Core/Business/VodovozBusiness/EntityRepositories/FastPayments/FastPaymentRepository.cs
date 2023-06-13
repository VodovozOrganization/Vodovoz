using System;
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
		
		public IList<FastPayment> GetAllPerformedOrProcessingFastPaymentsByOnlineOrder(
			IUnitOfWork uow, int onlineOrderId, decimal onlineOrderSum)
		{
			return uow.Session.QueryOver<FastPayment>()
				.Where(fp => fp.OnlineOrderId == onlineOrderId)
				.And(fp => fp.FastPaymentStatus == FastPaymentStatus.Performed || fp.FastPaymentStatus == FastPaymentStatus.Processing)
				.And(fp => fp.Amount == onlineOrderSum)
				.OrderBy(fp => fp.FastPaymentStatus).Desc
				.List();
		}

		public FastPaymentStatus? GetOrderFastPaymentStatus(IUnitOfWork uow, int orderId, int? onlineOrder = null)
		{
			var fastPayments = GetAllFastPaymentsByOrderOrderByStatusDesc(uow, orderId);

			if(fastPayments.Any())
			{
				return fastPayments.First().FastPaymentStatus;
			}

			var fastPaymentsByExternalId = GetPerformedFastPaymentByExternalId(uow, onlineOrder ?? -1);

			if(fastPaymentsByExternalId != null)
			{
				return fastPaymentsByExternalId.FastPaymentStatus;
			}

			return null;
		}

		public FastPayment GetFastPaymentByTicket(IUnitOfWork uow, string ticket)
		{
			return GetFastPaymentByTicketQuery(ticket)
				.GetExecutableQueryOver(uow.Session)
				.SingleOrDefault();
		}
		
		public FastPayment GetFastPaymentByGuid(IUnitOfWork uow, Guid fastPaymentGuid)
		{
			return uow.Session.QueryOver<FastPayment>()
				.Where(fp => fp.FastPaymentGuid == fastPaymentGuid)
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

		public FastPayment GetPerformedFastPaymentByExternalId(IUnitOfWork uow, int externalId)
		{
			return uow.Session.QueryOver<FastPayment>()
				.Where(fp => fp.FastPaymentStatus == FastPaymentStatus.Performed)
				.And(fp => fp.ExternalId == externalId)
				.SingleOrDefault();
		}

		public IList<FastPayment> GetAllPaymentsByOnlineOrder(IUnitOfWork uow, int orderId)
		{
			var result = uow.Session.QueryOver<FastPayment>()
				.Where(fp => fp.OnlineOrderId == orderId)
				.OrderBy(fp => fp.FastPaymentStatus).Desc
				.List();
			return result;
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
