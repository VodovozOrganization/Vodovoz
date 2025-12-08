using System.Linq;
using DateTimeHelpers;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using GLib;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Reports.WageCalculation.CallCenterMotivation;
using Vodovoz.ViewWidgets.Reports;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Views.Reports
{
	public partial class CallCenterMotivationReportView : TabViewBase<CallCenterMotivationReportViewModel>
	{
		public CallCenterMotivationReportView(CallCenterMotivationReportViewModel viewModel) : base(viewModel)
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

			ybuttonCreateReport.BindCommand(ViewModel.CreateReportCommand);
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

			yenumSlice.ItemsEnum = typeof(DateTimeSliceType);
			yenumSlice.ShowSpecialStateAll = false;
			yenumSlice.Binding
				.AddBinding(ViewModel, vm => vm.DateSlicingType, w => w.SelectedItem)
				.InitializeFromSource();

			ychkbtnShowDynamics.Binding
				.AddBinding(ViewModel, vm => vm.ShowDynamics, w => w.Active)
				.InitializeFromSource();

			ShowFilter();

			leftrightlistview.ViewModel = ViewModel.GroupingSelectViewModel;
			leftrightlistview.Sensitive = false;

			ViewModel.ShowReportAction = ShowReport;
			
			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;

			hpaned1.Position = 640;
		}
		
		private void ShowReport()
		{
			ConfigureTreeView();
			ytreeReportIndicatorsRows.ItemsDataSource = ViewModel.Report.DisplayRows;
			ytreeReportIndicatorsRows.YTreeModel.EmitModelChanged();
		}

		private void ConfigureTreeView()
		{
			var columnsConfig = FluentColumnsConfig<CallCenterMotivationReport.CallCenterMotivationReportRow>.Create();

			columnsConfig
				.AddColumn("")
				.AddTextRenderer(row => row.IsSubheaderRow ? "<b>№</b>" : row.Index, useMarkup: true);

			var firstColumnTitle = string.Join(" | ", ViewModel.Report.GroupingBy.Select(x => x.GetEnumTitle()));

			columnsConfig
				.AddColumn(firstColumnTitle)
				.AddTextRenderer(row => (row.IsSubheaderRow || row.IsTotalsRow) ? $"<b>{row.Title}</b>" : row.Title, useMarkup: true)
				.WrapWidth(350)
				.WrapMode(WrapMode.Word);

			if(ViewModel.Report.ShowDynamics)
			{
				for(var i = 0; i < ViewModel.Report.Slices.Count; i++)
				{
					var index = i;

					AddDateSliceColumn(columnsConfig, ViewModel.Report.Slices[index].ToString(), "Продано")
						.AddTextRenderer(row => row.IsSubheaderRow ? "" : $"{row.SliceColumnValues[index].Sold.ToFormattedUnitString(row.IsMoneyFormat)}");

					AddSimpleColumn(columnsConfig, "Премия")
						.AddTextRenderer(row => row.IsSubheaderRow ? "" : $"{row.SliceColumnValues[index].Premium.ToFormattedUnitString(true)}");

					AddSimpleColumn(columnsConfig, "Продано\nдинамика")
						.AddTextRenderer(row => row.IsSubheaderRow || index == 0 ? "-" : $"{row.DynamicColumns[index - 1].Sold.ToFormattedUnitString(row.IsMoneyFormat)}");

					AddSimpleColumn(columnsConfig, "Премия\nдинамика").
						AddTextRenderer(row => row.IsSubheaderRow || index == 0 ? "" : $"{row.DynamicColumns[index - 1].Premium.ToFormattedUnitString(true)}");
					
					AddColumnGroupSeparator(columnsConfig);
				}
			}
			else
			{
				for(var i = 0; i < ViewModel.Report.Slices.Count; i++)
				{
					var index = i;

					AddDateSliceColumn(columnsConfig, ViewModel.Report.Slices[index].ToString(), "Продано")
						.AddTextRenderer(row => row.IsSubheaderRow ? "" : $"{row.SliceColumnValues[index].Sold.ToFormattedUnitString(row.IsMoneyFormat)}");

					AddSimpleColumn(columnsConfig, "Премия")
						.AddTextRenderer(row => row.IsSubheaderRow ? "" : $"{row.SliceColumnValues[index].Premium.ToFormattedUnitString(true)}");

					AddColumnGroupSeparator(columnsConfig);
				}
			}

			AddDateSliceColumn(columnsConfig, "Всего за период", "Продано")
				.AddNumericRenderer(row => row.IsSubheaderRow ? "" : $"{row.SliceColumnValues.Sum(x => x.Sold).ToFormattedUnitString(row.IsMoneyFormat)}");

			AddSimpleColumn(columnsConfig, "Премия")
				.AddNumericRenderer(row => row.IsSubheaderRow ? "" : $"{row.SliceColumnValues.Sum(x => x.Premium).ToFormattedUnitString(true)}");

			columnsConfig.AddColumn("");

			ytreeReportIndicatorsRows.ColumnsConfig = columnsConfig.Finish();
			ytreeReportIndicatorsRows.EnableGridLines = TreeViewGridLines.Both;
		}

		private void AddColumnGroupSeparator(FluentColumnsConfig<CallCenterMotivationReport.CallCenterMotivationReportRow> columnsConfig)
		{
			var headerBg = ytreeReportIndicatorsRows.Style.Mid(StateType.Insensitive);

			var separatorColumn = columnsConfig.AddColumn("");
			separatorColumn.TreeViewColumn.Sizing = TreeViewColumnSizing.Fixed;
			separatorColumn.TreeViewColumn.FixedWidth = 5;

			separatorColumn
				.AddTextRenderer(row => "")
				.AddSetter((c, row) =>
				{
					c.CellBackgroundGdk = headerBg;
				});
		}

		private ColumnMapping<CallCenterMotivationReport.CallCenterMotivationReportRow> AddSimpleColumn(FluentColumnsConfig<CallCenterMotivationReport.CallCenterMotivationReportRow> columnsConfig, string title)
		{
			var column = columnsConfig.AddColumn("");
			
			var vbox = new VBox(false, 0);
			vbox.SetSizeRequest(-1, 50);

			var bottomLabel = new Label(title)
			{
				Xalign = 0.5f,
				Yalign = 1f
			};

			vbox.PackStart(new Label(""), true, true, 0);
			vbox.PackEnd(bottomLabel, false, false, 0);

			column.TreeViewColumn.Widget = vbox;
			
			vbox.ShowAll();

			return column;
		}

		private ColumnMapping<CallCenterMotivationReport.CallCenterMotivationReportRow> AddDateSliceColumn(
			FluentColumnsConfig<CallCenterMotivationReport.CallCenterMotivationReportRow> columnsConfig, 
			string topHeader,
			string bottomHeader)
		{
			var column = columnsConfig.AddColumn("");

			var vbox = new VBox(false, 0);

			vbox.SetSizeRequest(-1, 50);

			var topLabel = new Label
			{
				UseMarkup = true,
				Markup = $"<b>{Markup.EscapeText(topHeader)}</b>",
				Xalign = 0.5f,
				Yalign = 0f
			};

			var bottomLabel = new Label(bottomHeader)
			{
				Xalign = 0.5f,
				Yalign = 1f
			};
			
			var spacer = new Label("");

			vbox.PackStart(topLabel, false, false, 0);
			vbox.PackStart(spacer, true, true, 0);
			vbox.PackEnd(bottomLabel, false, false, 0);

			column.TreeViewColumn.Widget = vbox;
			
			vbox.ShowAll();

			return column;
		}
		

		private void ShowFilter()
		{
			var filterView = new IncludeExludeFiltersView(ViewModel.FilterViewModel);
			vboxParameters.Add(filterView);
			filterView.HeightRequest = ViewModel.FilterViewModel.Filters.Count * 21 + 70;
			filterView.Show();
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
			eventboxArrow.ButtonPressEvent -= OnEventboxArrowButtonPressEvent;
			base.Dispose();
		}
	}
}
