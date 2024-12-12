using QS.DomainModel.UoW;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	public interface IIncludeExcludeSalesFilterFactory
	{
		IncludeExludeFiltersViewModel CreateSalesReportIncludeExcludeFilter(IUnitOfWork unitOfWork, bool userIsSalesRepresentative);
		IncludeExludeFiltersViewModel CreateTurnoverOfWarehouseBalancesReportFilterViewModel(IUnitOfWork unitOfWork);
	}
}
