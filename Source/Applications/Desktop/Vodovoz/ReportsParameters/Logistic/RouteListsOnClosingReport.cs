using QS.Views;
using QS.Widgets;
using Vodovoz.ViewModels.ReportsParameters.Logistics;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class RouteListsOnClosingReport : ViewBase<RouteListsOnClosingReportViewModel>
	{
		public RouteListsOnClosingReport(RouteListsOnClosingReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			dateEnd.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.EndDate, w => w.DateOrNull)
				.InitializeFromSource();

			ycheckTodayRouteLists.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowTodayRouteLists, w => w.Active)
				.InitializeFromSource();

			nullCheckVisitingMasters.RenderMode = RenderMode.Icon;
			nullCheckVisitingMasters.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IncludeVisitingMasters, w => w.Active)
				.InitializeFromSource();

			ySpecCmbGeographicGroup.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.GeoGroups, w => w.ItemsList)
				.AddBinding(vm => vm.GeoGroup, w => w.SelectedItem)
				.InitializeFromSource();

			enumcheckCarTypeOfUse.EnumType = ViewModel.CarTypeOfUseType;
			enumcheckCarTypeOfUse.AddEnumToHideList(ViewModel.HiddenCarTypeOfUse);
			enumcheckCarTypeOfUse.SelectAll();
			enumcheckCarTypeOfUse.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CarTypeOfUseList, w => w.SelectedValues)
				.InitializeFromSource();

			enumcheckCarOwnType.EnumType = ViewModel.CarOwnTypeType;
			enumcheckCarOwnType.SelectAll();
			enumcheckCarOwnType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CarOwnTypeList, w => w.SelectedValues)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
