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
		/// Получить ноды для отчета по продажам
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="startDate">Дата начала периода</param>
		/// <param name="endDate">Дата окончания периода</param>
		/// <param name="orderDateType">Тип даты заказа</param>
		/// <param name="filters">Фильтры для отчета</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список нод для отчета по продажам</returns>
		Task<IList<SalesReportDataNode>> GetSalesReportData(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			OrderDateFilterType orderDateType,
			SalesReportFilters filters,
			CancellationToken cancellationToken);

		/// <summary>
		/// Получить ноду для бутылей
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="orderIds">Коллекция идентификаторов заказов</param>
		/// <param name="defaultBottleNomenclatureId">Идентификатор бутыли по умолчанию</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Нода бутылей для отчета по продажам</returns>
		Task<BottlesDataNode> GetBottlesData(
			IUnitOfWork uow,
			IEnumerable<int> orderIds,
			int defaultBottleNomenclatureId,
			CancellationToken cancellationToken);
	}
}
