using Gamma.ColumnConfig;
using Gamma.Widgets.Additions;
using Gtk;
using QS.Views.GtkUI;
using System.Linq;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Transport.Reports.IncorrectFuel;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Views.Reports
{
	public partial class IncorrectFuelReportView : TabViewBase<IncorrectFuelReportViewModel>
	{
		private const int _hpanedDefaultPosition = 440;
		private const int _hpanedMinimalPosition = 16;

		public IncorrectFuelReportView(IncorrectFuelReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			hpanedMain.Position = _hpanedDefaultPosition;

			daterangepickerPeriod.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			entityentryCar.ViewModel = ViewModel.CarEntityEntryViewModel;
			entityentryFuelCard.ViewModel = ViewModel.FuelCardEntityEntryViewModel;

			enumcheckCarTypeOfUse.EnumType = typeof(CarTypeOfUse);
			enumcheckCarTypeOfUse.Binding
				.AddBinding(ViewModel, vm => vm.CarTypesOfUse, w => w.SelectedValuesList, new EnumsListConverter<CarTypeOfUse>())
				.InitializeFromSource();

			enumcheckCarOwnType.EnumType = typeof(CarOwnType);
			enumcheckCarOwnType.Binding
				.AddBinding(ViewModel, vm => vm.CarOwnTypes, w => w.SelectedValuesList, new EnumsListConverter<CarOwnType>())
				.InitializeFromSource();

			ycheckbuttonExcludeOfficeWorkers.Binding
				.AddBinding(ViewModel, vm => vm.IsExcludeOfficeWorkers, w => w.Active)
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

			ConfigureDataTreeView();

			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;
		}

		private void ConfigureDataTreeView()
		{
			ytreeReportRows.ColumnsConfig = FluentColumnsConfig<IncorrectFuelReportRow>.Create()
				.AddColumn("№\nп/п").HeaderAlignment(0.5f).AddNumericRenderer(x => x.RowNumber).XAlign(0.5f)
				.AddColumn("Гос.\nномер").HeaderAlignment(0.5f).AddTextRenderer(x => x.CarRegNumber).XAlign(0.5f)
				.AddColumn("Модель\nавто").HeaderAlignment(0.5f).AddTextRenderer(x => x.CarModel).XAlign(0.5f)
				.AddColumn("Принадлежность\nавто").HeaderAlignment(0.5f).AddTextRenderer(x => x.CarOwnTypeString).XAlign(0.5f)
				.AddColumn("Тип авто").HeaderAlignment(0.5f).AddTextRenderer(x => x.CarTypeOfUseString).XAlign(0.5f)
				.AddColumn("Категория\nводителя").HeaderAlignment(0.5f).AddTextRenderer(x => x.DriverCategoryString).XAlign(0.5f)
				.AddColumn("Водитель").HeaderAlignment(0.5f).AddTextRenderer(x => x.DriverName).WrapWidth(150).WrapMode(WrapMode.WordChar).XAlign(0.5f)
				.AddColumn("Номер ТК").HeaderAlignment(0.5f).AddTextRenderer(x => x.FuelCardNumber).XAlign(0.5f)
				.AddColumn("Тип топлива\nпо ДВ").HeaderAlignment(0.5f).AddTextRenderer(x => x.CarFuelType).XAlign(0.5f)
				.AddColumn("Тип заправленного\nтоплива").HeaderAlignment(0.5f).AddTextRenderer(x => x.TransactionFuelType).XAlign(0.5f)
				.AddColumn("Кол-во заправленного\nтоплива").HeaderAlignment(0.5f).AddTextRenderer(x => x.TransactionLitersAmountString).XAlign(0.5f)
				.AddColumn("Дата и время\nзаправки").HeaderAlignment(0.5f).AddTextRenderer(x => x.TransactionDateTimeString).XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ytreeReportRows.Binding
				.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.Report != null, w => w.Visible)
				.AddFuncBinding(vm => vm.Report != null ? vm.Report.Rows : Enumerable.Empty<IncorrectFuelReportRow>(), w => w.ItemsDataSource)
				.InitializeFromSource();

			ytreeReportRows.EnableGridLines = TreeViewGridLines.Both;
		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			yvboxFilterContainer.Visible = !yvboxFilterContainer.Visible;

			hpanedMain.Position = yvboxFilterContainer.Visible ? _hpanedDefaultPosition : _hpanedMinimalPosition;

			UpdateSliderArrow();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = yvboxFilterContainer.Visible ? ArrowType.Left : ArrowType.Right;
		}
	}
}
