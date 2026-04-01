using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V4.Services
{
	public interface IOrderCancellationLogicService
	{
		/// <summary>
		/// Проверяет, можно ли отменить заказ в текущем статусе
		/// </summary>
		Result CanCancel(Order order);

		/// <summary>
		/// Применяет отмену заказа
		/// </summary>
		Task<Result<string>> ApplyCancellationAsync(
			Guid externalOrderId,
			Source source,
			string transactionId,
			CancellationToken cancellationToken);
	}
}
