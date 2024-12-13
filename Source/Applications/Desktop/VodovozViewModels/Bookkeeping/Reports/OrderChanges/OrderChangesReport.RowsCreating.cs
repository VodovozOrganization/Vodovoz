using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.OrderChanges
{
	public partial class OrderChangesReport
	{
		private async Task SetReportRows(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			try
			{
				Rows = await CreateReportRows(uow, cancellationToken);
			}
			catch(Exception ex)
			{
				throw;
			}
		}

		private async Task<IEnumerable<OrderChangesReportRow>> CreateReportRows(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			return new List<OrderChangesReportRow>();
		}
	}
}
