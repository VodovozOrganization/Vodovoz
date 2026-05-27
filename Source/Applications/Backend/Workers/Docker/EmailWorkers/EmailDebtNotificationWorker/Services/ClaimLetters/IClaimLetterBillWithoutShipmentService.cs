using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace EmailDebtNotificationWorker.Services.ClaimLetters
{
	public interface IClaimLetterBillWithoutShipmentService
	{
		/// <summary>
		/// Получить или создать счет без отгрузки на долг
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="clientId">Идентификатор клиента</param>
		/// <param name="organizationId">Идентификатор организации</param>
		/// <param name="debtSum">Сумма долга</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Счет без отгрузки на долг</returns>
		Task<OrderWithoutShipmentForDebt> GetOrCreateOrderWithoutShipmentForDebtAsync(
			IUnitOfWork uow,
			int clientId,
			int organizationId,
			decimal debtSum,
			CancellationToken cancellationToken);
	}
}
