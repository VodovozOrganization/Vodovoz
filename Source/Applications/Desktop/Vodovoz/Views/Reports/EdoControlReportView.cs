using Gamma.ColumnConfig;
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
		private const int _hpanedDefaultPosition = 530;
		private const int _hpanedMinimalPosition = 16;

		private IncludeExludeFiltersView _filterView;

		public EdoControlReportView(EdoControlReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			hpanedMain.Position = _hpanedDefaultPosition;

			leftrightlistview.ViewModel = ViewModel.GroupingSelectViewModel;

			datePeriodPicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ybuttonCreateReport.Binding
				.AddBinding(ViewModel, vm => vm.CanGenerateReport, w => w.Visible)
				.InitializeFromSource();

			ybuttonAbortCreateReport.Binding
				.AddBinding(ViewModel, vm => vm.CanAbortReport, w => w.Visible)
				.InitializeFromSource();

			ybuttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
			ybuttonAbortCreateReport.BindCommand(ViewModel.AbortReportGenerationCommand);
			ybuttonSave.BindCommand(ViewModel.SaveReportCommand);
			ybuttonHelp.BindCommand(ViewModel.ShowInfoCommand);

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
				.AddColumn("Номер\nв ЭДО").AddTextRenderer(x => x.EdoContainerId)
				.AddColumn("Клиент").AddTextRenderer(x => x.IsRootRow ? x.GroupTitle : x.ClientName).WrapWidth(400).WrapMode(WrapMode.WordChar)
				.AddSetter((cell, node) =>
				{
					if(node.IsRootRow)
					{
						cell.Markup = $"<b>{node.GroupTitle}</b>";
					}
				})
				.AddColumn("Номер заказа").AddTextRenderer(x => x.OrderId)
				.AddColumn("Номер МЛ").AddTextRenderer(x => x.RouteListId)
				.AddColumn("Дата").AddTextRenderer(x => x.DeliveryDate)
				.AddColumn("Статус документооборота").AddTextRenderer(x => x.EdoStatus)
				.AddColumn("Тип доставки").AddTextRenderer(x => x.OrderDeliveryType)
				.AddColumn("Тип переноса").AddTextRenderer(x => x.AddressTransferType)
				.Finish();

			ytreeReportRows.Binding
				.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.Report != null, w => w.Visible)
				.AddFuncBinding(vm => vm.Report != null ? vm.Report.Rows : Enumerable.Empty<EdoControlReportRow>(), w => w.ItemsDataSource)
				.InitializeFromSource();

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
	}
}
