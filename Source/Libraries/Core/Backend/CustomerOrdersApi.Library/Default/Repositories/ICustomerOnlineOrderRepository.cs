using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrders.Contracts.Default.Orders;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.Default.Repositories
{
	public interface ICustomerOnlineOrderRepository
	{
		IEnumerable<OrderDto> GetCounterpartyOnlineOrdersWithoutOrder(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);

		/// <summary>
		/// Проверят, что у клиента есть не отмененный онлайн-заказ, созданный из указанного источника
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="externalCounterpartyId">Внешний Id пользователя</param>
		/// <param name="counterpartyErpId">Id пользователя в ДВ</param>
		/// <param name="source">Источник онлайн заказа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<bool> IsClientHasNotCancelledOnlineOrdersFromSource(IUnitOfWork uow, Guid externalCounterpartyId, int counterpartyErpId, Source source, CancellationToken cancellationToken = default);
	}
}
