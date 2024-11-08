using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Views.GtkUI;
using System.Linq;
using Vodovoz.ViewModels.Bookkeeping.Reports.EdoControl;
using Vodovoz.ViewModels.Bookkeepping.Reports.EdoControl;
using Vodovoz.ViewWidgets.Reports;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Views.Reports
{
	public partial class EdoControlReportView : TabViewBase<EdoControlReportViewModel>
	{
		private const int _hpanedDefaultPosition = 428;
		private const int _hpanedMinimalPosition = 16;

		private IncludeExludeFiltersView _filterView;

		public EdoControlReportView(EdoControlReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			leftrightlistview.ViewModel = ViewModel.GroupingSelectViewModel;

			datePeriodPicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ybuttonAbortCreateReport.Binding
				.AddBinding(ViewModel, vm => vm.CanAbortReport, w => w.Visible)
				.InitializeFromSource();

			ybuttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
			ybuttonAbortCreateReport.BindCommand(ViewModel.AbortReportGenerationCommand);
			ybuttonSave.BindCommand(ViewModel.SaveReportCommand);

			ShowIncludeExludeFilter();
			leftrightlistview.ViewModel = ViewModel.GroupingSelectViewModel;

			ConfigureDataTreeView();

			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;
		}

		private void ShowIncludeExludeFilter()
		{
			_filterView?.Destroy();
			_filterView = new IncludeExludeFiltersView(ViewModel.FilterViewModel);
			yvboxParameters.Add(_filterView);
			_filterView.HeightRequest = ViewModel.FilterViewModel.Filters.Count * 21 + 70;
			_filterView.Show();
		}

		private void ConfigureDataTreeView()
		{
			ytreeReportRows.ColumnsConfig = FluentColumnsConfig<EdoControlReportRow>.Create()
				.AddColumn("Номер документооборота").AddTextRenderer(x => x.EdoContainerId == null ? "" : x.EdoContainerId.Value.ToString())
				.AddColumn("Клиент").AddTextRenderer(x => x.ClientName).WrapWidth(350).WrapMode(WrapMode.WordChar)
				.AddColumn("Номер заказа").AddNumericRenderer(x => x.OrderId)
				.AddColumn("Номер МЛ").AddNumericRenderer(x => x.RouteListId)
				.AddColumn("Дата").AddTextRenderer(x => x.DeliveryDate.ToString("dd.MM.yyyy"))
				.AddColumn("Статус документооборота").AddTextRenderer(x => x.EdoStatus.Value.GetEnumTitle())
				.AddColumn("Тип доставки").AddTextRenderer(x => x.OrderDeliveryType.GetEnumTitle())
				.AddColumn("Тип переноса").AddTextRenderer(x => x.AddressTransferType.GetEnumTitle())
				.Finish();

			ytreeReportRows.Binding
				.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.Report != null, w => w.Visible)
				.AddFuncBinding(vm => vm.Report != null ? vm.Report.Rows : Enumerable.Empty<EdoControlReportRow>(), w => w.ItemsDataSource)
				.InitializeFromSource();
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
	}
}
