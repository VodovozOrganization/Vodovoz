using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels.QualityControl.Reports;
using Vodovoz.ViewWidgets.Reports;
using static Vodovoz.ViewModels.Logistic.DriversStopLists.DriversStopListsViewModel;
using static Vodovoz.ViewModels.QualityControl.Reports.NumberOfComplaintsAgainstDriversReportViewModel;

namespace Vodovoz.QualityControl.Reports
{
	[ToolboxItem(true)]
	public partial class NumberOfComplaintsAgainstDriversReportView : TabViewBase<NumberOfComplaintsAgainstDriversReportViewModel>
	{
		private IncludeExludeFiltersView _filterView;

		public NumberOfComplaintsAgainstDriversReportView(NumberOfComplaintsAgainstDriversReportViewModel viewModel) : base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			btnReportInfo.Visible = false;

			datePeriodPicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			comboGeoGroup.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.GeoGroups, w => w.ItemsList)
				.AddBinding(vm => vm.SelectedGeoGroup, w => w.SelectedItem)
				.InitializeFromSource();

			comboComplaintResult.SetRenderTextFunc<ComplaintResultBase>(cr => cr.Name);
			comboComplaintResult.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.ComplaintResults, w => w.ItemsList)
				.AddBinding(vm => vm.SelectedComplaintResult, w => w.SelectedItem)
				.InitializeFromSource();

			yenumcomboSorting.ItemsEnum = typeof(ReportSortOrder);
			yenumcomboSorting.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SelectedReportSortOrder, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			ybuttonCreateReport.Clicked += (s, e) => ViewModel.GenerateReportCommand.Execute();

			ybuttonSave.Clicked += (s, e) => ViewModel.ExportReportCommand.Execute();

			ybuttonSave.Binding
				.AddBinding(ViewModel, vm => vm.CanExportReport, w => w.Sensitive)
				.InitializeFromSource();

			ConfigureTreeView();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;

			ShowFilter();
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(NumberOfComplaintsAgainstDriversReportViewModel.Report))
			{
				ytreeReportIndicatorsRows.ItemsDataSource = ViewModel.Report.DriverRows;
				ytreeSubdivisionRows.ItemsDataSource = ViewModel.Report.SubdivisionRows;
			}
		}

		private void ConfigureTreeView()
		{
			ytreeReportIndicatorsRows.CreateFluentColumnsConfig<NumberOfComplaintsAgainstDriversReport.Row>()
				.AddColumn("ФИО").AddTextRenderer(x => x.DriverFullName)
				.AddColumn("Кол-во рекламаций").AddNumericRenderer(x => x.ComplaintsCount)
				.AddColumn("Номера рекламаций").AddTextRenderer(x => x.ComplaintsList)
				.AddColumn("")
				.Finish();

			ytreeSubdivisionRows.CreateFluentColumnsConfig<NumberOfComplaintsAgainstDriversReport.SubdivisionRow>()
				.AddColumn("Подразделение").AddTextRenderer(x => x.Subdivision)
				.AddColumn("Кол-во рекламаций").AddNumericRenderer(x => x.ComplaintsCount)
				.AddColumn("")
				.Finish();
		}

		private void ShowFilter()
		{
			_filterView?.Destroy();
			_filterView = new IncludeExludeFiltersView(ViewModel.IncludeExcludeFilterViewModel);
			_filterView.HeightRequest = 200;
			vboxParameters.Add(_filterView);
			_filterView.Show();
		}
	}
}
