using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IOrderRepository
	{
		/// <summary>
		/// Получение заказов на передачу оборудования для отправки ЭДО
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="maxDaysFromDeliveryDate"></param>
		/// <param name="closingDocumentDeliveryScheduleId"></param>
		/// <param name="vodovozOrganizationId"></param>
		/// <param name="orderStatusesToSendUpd"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<IEnumerable<OrderEntity>> GetOrdersForEquipmentTransferEdoAsync(
			IUnitOfWork uow,
			int maxDaysFromDeliveryDate,
			int closingDocumentDeliveryScheduleId,
			IEnumerable<OrderStatus> orderStatusesToSendUpd,
			CancellationToken cancellationToken);
	}
}
