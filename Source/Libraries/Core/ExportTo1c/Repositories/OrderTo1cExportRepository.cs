using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders;

namespace ExportTo1c.Library.Repositories
{
	public class OrderTo1cExportRepository : IOrderTo1cExportRepository
	{
		public async Task<DateTime?> GetMaxLastExportDate(
			IUnitOfWork unitOfWork,
			CancellationToken cancellationToken)
		{
			var result = await unitOfWork.Session.Query<OrderTo1cExport>()
				.Where(x => x.LastExportDate.HasValue)
				.OrderByDescending(x => x.LastExportDate)
				.Select(x => x.LastExportDate)
				.FirstOrDefaultAsync(cancellationToken);

			return result;
		}

		public async Task<IList<OrderTo1cExport>> GetNewChangedOrdersForExportTo1cApi(
			IUnitOfWork unitOfWork,
			CancellationToken cancellationToken)
		{
			var result = await (
				from orderTo1сExport in unitOfWork.Session.Query<OrderTo1cExport>()
				where
						orderTo1сExport.LastExportDate == null ||
						orderTo1сExport.LastOrderChangeDate > orderTo1сExport.LastExportDate
				select orderTo1сExport
			)
			.ToListAsync(cancellationToken);

			return result;
		}
	}
}
