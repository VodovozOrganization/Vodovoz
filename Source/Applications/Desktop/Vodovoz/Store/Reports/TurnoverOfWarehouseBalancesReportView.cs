using DateTimeHelpers;
using Gtk;
using QS.Dialog;
using QS.Views.Dialog;
using System;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Presentation.ViewModels.Store.Reports;
using Vodovoz.ViewWidgets.Reports;
using static Vodovoz.Presentation.ViewModels.Store.Reports.TurnoverOfWarehouseBalancesReport;

namespace Vodovoz.Store.Reports
{
	[ToolboxItem(true)]
	public partial class TurnoverOfWarehouseBalancesReportView : DialogViewBase<TurnoverOfWarehouseBalancesReportViewModel>
	{
		private IncludeExludeFiltersView _filterView;
		private int _hpanedDefaultPosition = 680;
		private int _hpanedMinimalPosition = 16;
		private readonly IGuiDispatcher _guiDispatcher;

		public TurnoverOfWarehouseBalancesReportView(
			TurnoverOfWarehouseBalancesReportViewModel viewModel,
			IGuiDispatcher guiDispatcher)
			: base(viewModel)
		{
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			Build();

			Initialize();
		}

		private void Initialize()
		{
			datePeriodPicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			yenumSlice.ItemsEnum = typeof(DateTimeSliceType);
			yenumSlice.ShowSpecialStateAll = false;
			yenumSlice.Binding
				.AddBinding(ViewModel, vm => vm.SlicingType, w => w.SelectedItem)
				.InitializeFromSource();

			btnReportInfo.BindCommand(ViewModel.ShowInfoCommand);

			ybuttonSave.BindCommand(ViewModel.SaveCommand);

			ybuttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);

			ybuttonCreateReport.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanGenerateReport, w => w.Visible)
				.InitializeFromSource();

			ybuttonAbortCreateReport.BindCommand(ViewModel.AbortCreateCommand);

			ybuttonAbortCreateReport.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanCancelGenerateReport, w => w.Visible)
				.InitializeFromSource();

			ShowFilter();

			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;

			hpaned1.Position = _hpanedDefaultPosition;

			UpdateSliderArrow();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Report))
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					RefreshReportPreview();
					QueueDraw();
				});
			}
		}

		private void RefreshReportPreview()
		{
			var columnsConfig = Gamma.ColumnConfig.FluentColumnsConfig<TurnoverOfWarehouseBalancesReportRow>.Create();

			columnsConfig.AddColumn("Склад").AddTextRenderer(x => x.WarehouseName)
				.AddColumn("Номенклатура").AddTextRenderer(x => x.NomanclatureName);

			for(var i = 0; i < ViewModel.Report.SliceTitles.Count(); i++)
			{
				var index = i;

				columnsConfig.AddColumn(ViewModel.Report.SliceTitles[index])
					.AddTextRenderer(row => row.SliceValues[index].ToString());
			}

			columnsConfig.AddColumn("Всего за период").AddTextRenderer(row => row.Total);

			columnsConfig.AddColumn("");

			ytreeReportIndicatorsRows.ColumnsConfig = columnsConfig.Finish();
			ytreeReportIndicatorsRows.EnableGridLines = TreeViewGridLines.Both;

			ytreeReportIndicatorsRows.ItemsDataSource = ViewModel.Report.ReportRows;
		}

		private void ShowFilter()
		{
			_filterView?.Destroy();
			_filterView = new IncludeExludeFiltersView(ViewModel.FilterViewModel);
			vboxParameters.Add(_filterView);
			_filterView.HeightRequest = ViewModel.FilterViewModel.Filters.Count * 21 + 70;
			_filterView.Show();
		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			scrolledwindow1.Visible = !scrolledwindow1.Visible;

			hpaned1.Position = scrolledwindow1.Visible ? _hpanedDefaultPosition : _hpanedMinimalPosition;

			UpdateSliderArrow();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = scrolledwindow1.Visible ? ArrowType.Left : ArrowType.Right;
		}
	}
}
