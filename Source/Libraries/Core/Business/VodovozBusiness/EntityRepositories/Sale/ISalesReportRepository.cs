using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VodovozBusiness.Nodes.SalesReport;

namespace VodovozBusiness.EntityRepositories.Sale
{
	public interface ISalesReportRepository
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
		Task<IList<SalesReportDataNode>> GetSalesReportData(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			string orderDateType,
			SalesReportFilters filters,
			CancellationToken cancellationToken);

		/// <summary>
		/// Получить ноды для бутылей
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="orderIds">Коллекция идентификаторов заказов</param>
		/// <param name="defaultBottleNomenclatureId">Идентификатор бутыли по умолчанию</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Ноды бутылей для отчета</returns>
		Task<BottlesDataNode> GetBottlesData(
			IUnitOfWork uow,
			IEnumerable<int> orderIds,
			int defaultBottleNomenclatureId,
			CancellationToken cancellationToken);
	}
}
