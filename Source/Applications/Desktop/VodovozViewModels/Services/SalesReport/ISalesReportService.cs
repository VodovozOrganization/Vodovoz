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
		/// Получить данные для отчета по продажам в виде списка нод SalesReportDataNode
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="startDate">Дата начала периода</param>
		/// <param name="endDate">Дата окончания периода</param>
		/// <param name="orderDateType">Тип даты заказа</param>
		/// <param name="filters">Фильтры для отчета</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список нод для отчета по продажам</returns>
		Task<IList<SalesReportDataNode>> GetSalesReportDataAsync(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			OrderDateFilterType orderDateType,
			SalesReportFilters filters,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Получить данные для бутылей в виде ноды BottlesDataNode
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="orderIds">Коллекция идентификаторов заказов</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Нода бутылей для отчета по продажам</returns>
		Task<BottlesDataNode> GetBottlesDataAsync(
			IUnitOfWork uow,
			IEnumerable<int> orderIds,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Экспортировать дерево отчета в Excel
		/// </summary>
		/// <param name="tree"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <param name="groupingTitle"></param>
		/// <param name="ordersCount"></param>
		/// <param name="planBottles"></param>
		/// <param name="factBottles"></param>
		/// <returns></returns>
		byte[] ExportToExcel(
			IList<SalesReportTreeNode> tree,
			DateTime startDate,
			DateTime endDate,
			string groupingTitle,
			int ordersCount,
			int planBottles,
			int factBottles);
	}
}
