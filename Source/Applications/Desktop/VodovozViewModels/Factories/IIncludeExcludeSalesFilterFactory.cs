using QS.DomainModel.UoW;
using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.ViewModels.Factories
{
	public interface IIncludeExcludeSalesFilterFactory
	{
		IncludeExludeFiltersViewModel CreateSalesReportIncludeExcludeFilter(IUnitOfWork unitOfWork, bool userIsSalesRepresentative);
	}
}