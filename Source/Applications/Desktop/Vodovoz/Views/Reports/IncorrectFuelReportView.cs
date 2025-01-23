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
				.AddColumn("№").AddNumericRenderer(x => x.RowNumber)
				.AddColumn("Гос.\nномер").AddTextRenderer(x => x.CarRegNumber)
				.AddColumn("Модель\nавто").AddTextRenderer(x => x.CarModel)
				.AddColumn("Принадлежность\nавто").AddTextRenderer(x => x.CarOwnTypeString)
				.AddColumn("Тип авто").AddTextRenderer(x => x.CarTypeOfUseString)
				.AddColumn("Категория\nводителя").AddTextRenderer(x => x.DriverCategoryString)
				.AddColumn("Водитель").AddTextRenderer(x => x.DriverName).WrapWidth(150).WrapMode(WrapMode.WordChar)
				.AddColumn("Номер ТК").AddTextRenderer(x => x.FuelCardNumber)
				.AddColumn("Тип топлива\nпо ДВ").AddTextRenderer(x => x.CarFuelType)
				.AddColumn("Тип заправленного\nтоплива").AddTextRenderer(x => x.TransactionFuelType)
				.AddColumn("Кол-во заправленного\nтоплива").AddTextRenderer(x => x.TransactionLitersAmountString)
				.AddColumn("Дата и время\nзаправки").AddTextRenderer(x => x.TransactionDateTimeString)
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
