using QS.DomainModel.UoW;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	/// <summary>
	/// Фабрика для создания фильтров include/exclude для отчетов
	/// </summary>
	public interface IIncludeExcludeSalesFilterFactory
	{
		/// <summary>
		/// Создает фильтр include/exclude для отчета по продажам
		/// </summary>
		/// <param name="unitOfWork">Unit of Work для работы с данными</param>
		/// <param name="onlyEmployeeId">Идентификатор сотрудника, для которого нужно ограничить видимость данных 
		/// Если передано значение, то пользователь сможет видеть только данные этого сотрудника
		/// Если null, то никаких ограничений по сотрудникам</param>
		/// <returns>Модель представления фильтров include/exclude для отчета по продажам</returns>
		IncludeExludeFiltersViewModel CreateSalesReportIncludeExcludeFilter(IUnitOfWork unitOfWork, int? onlyEmployeeId);

		/// <summary>
		/// Создает фильтр для отчета "Оборачиваемость складских остатков"
		/// </summary>
		/// <param name="unitOfWork">Unit of Work для работы с данными</param>
		/// <returns>Модель представления фильтров include/exclude для отчета "Оборачиваемость складских остатков"</returns>
		IncludeExludeFiltersViewModel CreateTurnoverOfWarehouseBalancesReportFilterViewModel(IUnitOfWork unitOfWork);

		/// <summary>
		/// Создает фильтр include/exclude для отчета по мотивации КЦ
		/// </summary>
		/// <param name="unitOfWork">Unit of Work для работы с данными</param>
		/// <param name="onlyEmployeeId">Идентификатор сотрудника, для которого нужно ограничить видимость данных
		/// Если передано значение, то пользователь сможет видеть только данные этого сотрудника
		/// Если null, то никаких ограничений по сотрудникам</param>
		/// <returns>Модель представления фильтров include/exclude для отчета по мотивации КЦ</returns>
		IncludeExludeFiltersViewModel CreateCallCenterMotivationReportIncludeExcludeFilter(IUnitOfWork unitOfWork, int? onlyEmployeeId);
	}
}
