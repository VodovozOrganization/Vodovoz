using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IEdoRepository
	{
		Task<IEnumerable<OrganizationEntity>> GetEdoOrganizationsAsync(CancellationToken cancellationToken);
		Task<IEnumerable<GtinEntity>> GetGtinsAsync(CancellationToken cancellationToken);
		Task<IEnumerable<GroupGtinEntity>> GetGroupGtinsAsync(CancellationToken cancellationToken);
		Task<bool> HasReceiptOnSumToday(decimal sum, CancellationToken cancellationToken);

		/// <summary>
		/// Получает ЭДО документы заказа по идентификатору заказа
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="orderId"></param>
		/// <returns></returns>
		IEnumerable<OrderEdoDocument> GetOrderEdoDocumentsByOrderId(IUnitOfWork uow, int orderId);
		IEnumerable<OrderEdoTask> GetEdoTaskByOrderAsync(IUnitOfWork uow, int orderId);
	}
}
