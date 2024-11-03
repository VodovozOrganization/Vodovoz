using QS.DomainModel.UoW;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	public interface IIncludeExcludeBookkeeppingReportsFilterFactory
	{
		IncludeExludeFiltersViewModel CreateEdoControlReportIncludeExcludeFilter(IUnitOfWork unitOfWork);
	}
}
