using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Orders;

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
			Export1cMode mode,
			CancellationToken cancellationToken)
		{
			if(mode != Export1cMode.ComplexAutomation)
			{
				return new List<OrderTo1cExport>();
			}

			// Выгружаем только новый безнал, либо любой тип оплаты, если уже выгружался ранее(мог измениться тип оплаты (н-р: с безнала на нал)

			var result = await (
				from orderTo1cExport in unitOfWork.Session.Query<OrderTo1cExport>()
				join o in unitOfWork.Session.Query<OrderEntity>()
					on orderTo1cExport.OrderId equals o.Id into orders
				from order in orders.DefaultIfEmpty()

				let lastExportDate = orderTo1cExport.LastExportDate
				let isFirstExport = lastExportDate == null
				let isCashless = order != null && order.PaymentType == PaymentType.Cashless
				let hasChanges = orderTo1cExport.LastOrderChangeDate > lastExportDate

				where (isFirstExport && isCashless) || (!isFirstExport && hasChanges)

				select orderTo1cExport
			)
			.ToListAsync(cancellationToken);

			return result;
		}
	}
}
