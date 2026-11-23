using QS.ViewModels.Widgets;
using Vodovoz.Models.Cash;
using Vodovoz.ViewModels.ReportsParameters.Profitability;

namespace Vodovoz.ViewModels.Factories
{
	public interface ILeftRightListViewModelFactory
	{
		LeftRightListViewModel<GroupingNode> CreateSalesReportGroupingsConstructor();
		LeftRightListViewModel<GroupingNode> CreateSalesWithDynamicsReportGroupingsConstructor();

		/// <summary>
		/// Настраивает список группировок отчёта ДДС и ДДР
		/// </summary>
		/// <returns>Модель выбора группировок</returns>
		LeftRightListViewModel<CashFlowReportGrouping> CreateCashFlowAnalysisReportGroupingsConstructor();

		void SetDefaultLeftItemsForSalesWithDynamicsReportGroupings(LeftRightListViewModel<GroupingNode> leftRightListViewModel);
		void SetDefaultLeftItemsForCallCenterMotivationReportGroupings(LeftRightListViewModel<GroupingNode> leftRightListViewModel);
		LeftRightListViewModel<GroupingNode> CreateCompletedDriverEventsSortingConstructor();
		LeftRightListViewModel<GroupingNode> CreateEdoControlReportGroupingsConstructor();
		LeftRightListViewModel<GroupingNode> CreateCallCenterMotivationReportGroupingsConstructor();
	}
}
