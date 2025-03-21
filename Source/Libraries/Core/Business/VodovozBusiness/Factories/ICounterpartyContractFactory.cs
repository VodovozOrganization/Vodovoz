using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Factories
{
	public interface ICounterpartyContractFactory
	{
		CounterpartyContract CreateContract(IUnitOfWork unitOfWork, Order order, Organization organization = null);
	}
}
