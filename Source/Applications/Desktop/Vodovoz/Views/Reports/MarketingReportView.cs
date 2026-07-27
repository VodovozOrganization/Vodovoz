using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Reports.Sales;
using Vodovoz.ViewWidgets.Reports;

namespace Vodovoz.ReportsParameters.Sales
{
	public partial class MarketingReportView : TabViewBase<MarketingReportViewModel>
	{
		private IncludeExludeFiltersView _filterView;

		public MarketingReportView(MarketingReportViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
			UpdateSliderArrow();
		}

		private void ConfigureDlg()
		{
			btnReportInfo.BindCommand(ViewModel.ShowInfoCommand);

			ybuttonSave.BindCommand(ViewModel.SaveReportCommand);
			ybuttonSave.Binding
				.AddBinding(ViewModel, vm => vm.SaveProgressText, w => w.Label)
				.AddBinding(vm => vm.CanSave, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
			ybuttonCreateReport.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => !vm.IsGenerating, w => w.Visible)
				.InitializeFromSource();

			ybuttonAbortCreateReport.BindCommand(ViewModel.AbortCreateReportCommand);
			ybuttonAbortCreateReport.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsGenerating, w => w.Visible)
				.InitializeFromSource();

			datePeriodPicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			yenumGrouping.ItemsEnum = typeof(MarketingReportGroupingType);
			yenumGrouping.ShowSpecialStateAll = false;
			yenumGrouping.Binding
				.AddBinding(ViewModel, vm => vm.GroupingType, w => w.SelectedItem)
				.InitializeFromSource();

			yenumDateType.ItemsEnum = typeof(MarketingReportDateType);
			yenumDateType.ShowSpecialStateAll = false;
			yenumDateType.Binding
				.AddBinding(ViewModel, vm => vm.DateType, w => w.SelectedItem)
				.InitializeFromSource();

			ShowFilter();
			ytreeReportIndicatorsRows.RowActivated += OnReportRowActivated;
			ViewModel.ShowReportAction = ShowReport;
			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;
			hpaned1.Position = 500;
		}

		private void ShowReport()
		{
			var columnsConfig = FluentColumnsConfig<MarketingReportDisplayRow>.Create();
			columnsConfig.AddColumn("Показатель")
				.AddTextRenderer(row => row.IsSection ? $"<b>{row.Title}</b>" : row.Title, useMarkup: true)
				.WrapWidth(350);
			columnsConfig.AddColumn("Значение")
				.AddTextRenderer(row => row.Value)
				.XAlign(1);
			columnsConfig.AddColumn("Дополнительно")
				.AddTextRenderer(row => row.AdditionalValue ?? string.Empty)
				.XAlign(1);
			columnsConfig.AddColumn("");

			ytreeReportIndicatorsRows.ColumnsConfig = columnsConfig.Finish();
			ytreeReportIndicatorsRows.EnableGridLines = TreeViewGridLines.Both;
			ytreeReportIndicatorsRows.ItemsDataSource = ViewModel.Report?.DisplayRows;
			ytreeReportIndicatorsRows.YTreeModel.EmitModelChanged();
		}

		private void ShowFilter()
		{
			_filterView?.Destroy();
			_filterView = new IncludeExludeFiltersView(ViewModel.FilterViewModel);
			vboxParameters.Add(_filterView);
			_filterView.HeightRequest = ViewModel.FilterViewModel.Filters.Count * 21 + 70;
			_filterView.Show();
		}

		private void OnReportRowActivated(object o, RowActivatedArgs args)
		{
			var row = ytreeReportIndicatorsRows.GetSelectedObject<MarketingReportDisplayRow>();
			if(row == null)
			{
				return;
			}

			GetClipboard(Gdk.Selection.Clipboard).Text = $"{row.Title}\t{row.Value}\t{row.AdditionalValue}";
		}

		private void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			scrolledwindow2.Visible = !scrolledwindow2.Visible;
			hpaned1.PositionSet = false;
			UpdateSliderArrow();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = scrolledwindow2.Visible ? ArrowType.Left : ArrowType.Right;
		}

		public override void Dispose()
		{
			ytreeReportIndicatorsRows.RowActivated -= OnReportRowActivated;
			eventboxArrow.ButtonPressEvent -= OnEventboxArrowButtonPressEvent;
			base.Dispose();
		}
	}
}
