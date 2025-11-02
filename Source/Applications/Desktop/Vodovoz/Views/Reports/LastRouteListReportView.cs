using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using System.Linq;
using Vodovoz.ViewModels.ViewModels.Reports.Logistics.LastRouteListReport;
using Vodovoz.ViewWidgets.Reports;
namespace Vodovoz.Views.Reports
{
	public partial class LastRouteListReportView : TabViewBase<LastRouteListReportViewModel>
	{
		private const int _hpanedDefaultPosition = 530;
		private const int _hpanedMinimalPosition = 16;

		private IncludeExludeFiltersView _filterView;

		public LastRouteListReportView(LastRouteListReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			hpanedMain.Position = _hpanedDefaultPosition;

			ybuttonCreateReport.Binding
				.AddBinding(ViewModel, vm => vm.CanGenerateReport, w => w.Visible)
				.InitializeFromSource();

			ybuttonAbortCreateReport.Binding
				.AddBinding(ViewModel, vm => vm.CanAbortReport, w => w.Visible)
				.InitializeFromSource();

			ybuttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
			ybuttonAbortCreateReport.BindCommand(ViewModel.AbortReportGenerationCommand);
			ybuttonSave.BindCommand(ViewModel.SaveReportCommand);

			firedPicker.Binding.AddSource(ViewModel)

				.AddBinding(vm => vm.FiredStartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.FiredEndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			firstWorkingDayPicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.FirstWorkDayStartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.FirstWorkDayEndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			lastRLPicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.LastRouteListStartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.LastRouteListEndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			calculationPicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CalculateStartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.CalculateEndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			hiredPicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.HiredStartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.HiredEndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ConfigureDataTreeView();

			ShowIncludeExludeFilter();

			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;

			UpdateSliderArrow();
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
			ytreeReportRows.ColumnsConfig = FluentColumnsConfig<LastRouteListReportRow>.Create()
				.AddColumn("№ п/п").AddNumericRenderer(r => r.RowNum)
				.AddColumn("ФИО сотрудника").AddTextRenderer(r => r.DriverFio)
				.AddColumn("Статус сотрудника").AddEnumRenderer(r => r.DriverStatus)
				.AddColumn("Первый раб.день").AddDateRenderer(r => r.FirstWorkDay)
				.AddColumn("Дата приема").AddDateRenderer(r => r.DateHired)
				.AddColumn("Дата увольнения").AddDateRenderer(r => r.DateFired)
				.AddColumn("Дата расчета").AddDateRenderer(r => r.DateCalculated)
				.AddColumn("Последний МЛ").AddNumericRenderer(r => r.LastRouteListId)
				.AddColumn("Дата последнего доставленного МЛ").AddDateRenderer(r => r.LastClosedRouteListDate)
				.AddColumn("Кол-во дней прошло\nот последнего МЛ").AddNumericRenderer(r => r.DaysCountFromLastClosedRouteList)
				.AddColumn("Категория").AddTextRenderer(r => r.EmployeeCategoryString)
				.AddColumn("Управляет а/м типа").AddTextRenderer(r => r.CarTypeOfUseString)
				.AddColumn("Принадлежность").AddTextRenderer(r => r.CarsOwnString)
				.AddColumn("")
				.Finish();

			ytreeReportRows.Binding
				.AddFuncBinding(ViewModel, vm => vm.Report.Rows, w => w.ItemsDataSource)
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

		public override void Destroy()
		{
			eventboxArrow.ButtonPressEvent -= OnEventboxArrowButtonPressEvent;
			base.Destroy();
		}
	}
}
