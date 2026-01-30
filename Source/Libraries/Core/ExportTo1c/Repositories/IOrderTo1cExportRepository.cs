using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders;

namespace ExportTo1c.Library.Repositories
{
	public interface IOrderTo1cExportRepository
	{
		Task<DateTime?> GetMaxLastExportDate(IUnitOfWork unitOfWork, CancellationToken cancellationToken);

		/// <summary>
		/// Получение заказов с изменёнными значимыми для 1с полями
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<IList<OrderTo1cExport>> GetNewChangedOrdersForExportTo1cApi(
		   IUnitOfWork unitOfWork,
		   CancellationToken cancellationToken);
	}
}
