using QS.DomainModel.UoW;
using static Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters.IncludeExcludeLastRouteListFilterFactory;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	public interface IIncludeExcludeLastRouteListFilterFactory
	{
		IncludeExludeFiltersViewModel CreateLastReportIncludeExcludeFilter(IUnitOfWork unitOfWork, LastRouteListInitIncludeFilter initIncludeFilter);
	}
}
