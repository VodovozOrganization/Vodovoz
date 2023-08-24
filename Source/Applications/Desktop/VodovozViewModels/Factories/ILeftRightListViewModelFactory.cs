using QS.ViewModels.Widgets;
using Vodovoz.ViewModels.ReportsParameters.Profitability;

namespace Vodovoz.ViewModels.Factories
{
	public interface ILeftRightListViewModelFactory
	{
		LeftRightListViewModel<GroupingNode> CreateSalesReportGroupingsConstructor();
	}
}
