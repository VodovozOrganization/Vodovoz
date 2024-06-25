using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories.ServiceClaims;

namespace Vodovoz.Infrastructure.Persistance.ServiceClaims
{
	internal sealed class ServiceClaimRepository : IServiceClaimRepository
	{
		public IList<ServiceClaim> GetServiceClaimForOrder(IUnitOfWork uow, Domain.Orders.Order order)
		{
			ServiceClaim serviceClaimAlias = null;
			Domain.Orders.Order initialOrderAlias = null, finalOrderAlias = null;

			var queryOver = uow.Session.QueryOver(() => serviceClaimAlias)
				.JoinAlias(s => s.InitialOrder, () => initialOrderAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(s => s.FinalOrder, () => finalOrderAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where(s => initialOrderAlias.Id == order.Id || finalOrderAlias.Id == order.Id);

			return queryOver.List<ServiceClaim>();
		}

		public QueryOver<ServiceClaim> GetDoneClaimsForClient(Domain.Orders.Order order)
		{
			ServiceClaim serviceClaimAlias = null;
			Counterparty counterpartyAlias = null;

			var queryOver = QueryOver.Of(() => serviceClaimAlias)
				.JoinAlias(s => s.Counterparty, () => counterpartyAlias)
				.Where(s => counterpartyAlias.Id == order.Client.Id
					&& s.Status == ServiceClaimStatus.Ready
					&& s.FinalOrder == null
					&& s.Payment == order.PaymentType);

			return queryOver;
		}
	}
}
