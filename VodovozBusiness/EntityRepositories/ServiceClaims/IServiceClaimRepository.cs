using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Service;

namespace Vodovoz.EntityRepositories.ServiceClaims
{
	public interface IServiceClaimRepository
	{
		IList<ServiceClaim> GetServiceClaimForOrder(IUnitOfWork uow, Vodovoz.Domain.Orders.Order order);
		QueryOver<ServiceClaim> GetDoneClaimsForClient(Vodovoz.Domain.Orders.Order order);
	}
}