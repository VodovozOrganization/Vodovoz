using QS.DomainModel.UoW;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	public interface IIncludeExcludeBookkeepingReportsFilterFactory
	{
		IncludeExludeFiltersViewModel CreateEdoControlReportIncludeExcludeFilter(IUnitOfWork unitOfWork);
	}
}
