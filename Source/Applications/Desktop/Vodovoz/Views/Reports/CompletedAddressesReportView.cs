using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.ViewModels.ViewModels.Reports.Logistics.CompletedAddressesReport;
using Vodovoz.ViewWidgets.Reports;

namespace Vodovoz.Views.Reports
{
	public partial class CompletedAddressesReportView : TabViewBase<CompletedAddressesReportViewModel>
	{
		private const int _hpanedDefaultPosition = 530;
		private const int _hpanedMinimalPosition = 16;
		private const int _filterHeight = 400;

		private IncludeExludeFiltersView _filterView;

		public CompletedAddressesReportView(CompletedAddressesReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			hpanedMain.Position = _hpanedDefaultPosition;

			datePeriodPicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.AddBinding(vm => vm.CanChangePeriod, w => w.Sensitive)
				.InitializeFromSource();

			yvboxParameters.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeFilters, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
			ybuttonAbortCreateReport.BindCommand(ViewModel.AbortReportGenerationCommand);
			ybuttonExport.BindCommand(ViewModel.ExportReportCommand);

			ShowIncludeExludeFilter();

			ConfigureDataTreeView();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;

			UpdateSliderArrow();
		}

		private void ShowIncludeExludeFilter()
		{
			_filterView?.Destroy();
			_filterView = new IncludeExludeFiltersView(ViewModel.FilterViewModel);
			yvboxParameters.Add(_filterView);
			_filterView.HeightRequest = _filterHeight;
			_filterView.Show();
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Report))
			{
				ConfigureDataTreeView();
			}
		}

		private void ConfigureDataTreeView()
		{
			var report = ViewModel.Report;

			var columnsConfig = FluentColumnsConfig<CompletedAddressesReportRow>.Create()
				.AddColumn("ФИО").AddTextRenderer(r => r.DriverFullName);

			if(report != null)
			{
				for(var i = 0; i < report.DistrictTitles.Count; i++)
				{
					var districtIndex = i;

					columnsConfig
						.AddColumn(report.DistrictTitles[i])
						.AddNumericRenderer(r => r.DistrictCounts[districtIndex]);
				}
			}

			ytreeReportRows.ColumnsConfig = columnsConfig
				.AddColumn("Суммарно адресов по всем районам").AddNumericRenderer(r => r.Total)
				.AddColumn("")
				.Finish();

			ytreeReportRows.ItemsDataSource = report?.Rows;

			ytreeReportRows.EnableGridLines = TreeViewGridLines.Both;
		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			yvboxParameters.Visible = !yvboxParameters.Visible;

			hpanedMain.Position = yvboxParameters.Visible ? _hpanedDefaultPosition : _hpanedMinimalPosition;

			UpdateSliderArrow();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = yvboxParameters.Visible ? ArrowType.Left : ArrowType.Right;
		}

		public override void Destroy()
		{
			ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
			eventboxArrow.ButtonPressEvent -= OnEventboxArrowButtonPressEvent;
			base.Destroy();
		}
	}
}
