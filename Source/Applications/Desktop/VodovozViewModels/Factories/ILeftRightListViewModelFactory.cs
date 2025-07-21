using QS.ViewModels.Widgets;
using Vodovoz.ViewModels.ReportsParameters.Profitability;

namespace Vodovoz.ViewModels.Factories
{
	public interface ILeftRightListViewModelFactory
	{
		LeftRightListViewModel<GroupingNode> CreateSalesReportGroupingsConstructor();
		LeftRightListViewModel<GroupingNode> CreateSalesWithDynamicsReportGroupingsConstructor();
		void SetDefaultLeftItemsForSalesWithDynamicsReportGroupings(LeftRightListViewModel<GroupingNode> leftRightListViewModel);
		LeftRightListViewModel<GroupingNode> CreateCompletedDriverEventsSortingConstructor();
		LeftRightListViewModel<GroupingNode> CreateEdoControlReportGroupingsConstructor();
	}
}
