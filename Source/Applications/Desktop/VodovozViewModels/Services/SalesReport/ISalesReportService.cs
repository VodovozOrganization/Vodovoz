using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VodovozBusiness.Nodes.SalesReport;

namespace Vodovoz.ViewModels.Services.SalesReport
{
	public interface ISalesReportService
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <param name="orderDateType"></param>
		/// <param name="filters"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<IList<SalesReportDataNode>> GetSalesReportDataAsync(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			string orderDateType,
			SalesReportFilters filters,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="orderIds"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<BottlesDataNode> GetBottlesDataAsync(
			IUnitOfWork uow,
			IEnumerable<int> orderIds,
			CancellationToken cancellationToken = default);
	}
}
