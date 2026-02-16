using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using System;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.ViewModels.Reports.Logistics.AverageFlowDiscrepanciesReport;
using Vodovoz.ViewWidgets.Reports;

namespace Vodovoz.Views.Reports
{
	public partial class AverageFlowDiscrepanciesReportView : TabViewBase<AverageFlowDiscrepanciesReportViewModel>
	{
		public AverageFlowDiscrepanciesReportView(AverageFlowDiscrepanciesReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}		

		private void Configure()
		{
			rangeBulkEmailEventDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDate)
				.AddBinding(vm => vm.EndDate, w => w.EndDate)
				.InitializeFromSource();

			yspinDiscrepancy.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DiscrepancyPercentFilter, w => w.ValueAsInt)
				.InitializeFromSource();

			ybuttonCreateReport.BindCommand(ViewModel.CreateReportCommand);
			ybuttonSave.BindCommand(ViewModel.SaveReportCommand);

			ytreeviewReport.ColumnsConfig = FluentColumnsConfig<AverageFlowDiscrepanciesReportRow>.Create()
				.AddColumn("Авто").AddTextRenderer(ev => ev.Car)
				.AddColumn("Начальная калибровка").AddDateRenderer(ev => ev.CalibrationDate)
				.AddColumn("Конечная калибровка").AddDateRenderer(ev => ev.NextCalibrationDate)
				.AddColumn("Начальный баланс").AddNumericRenderer(ev => ev.CurrentBalance).Digits(2)
				.AddColumn("Актуальный баланс").AddNumericRenderer(ev => ev.ActualBalance).Digits(2)
				.AddColumn("Подтвержденный\nкилометраж").AddNumericRenderer(ev => ev.ConfirmedDistance).Digits(2)
				.AddColumn("Пересчитанный\nкилометраж").AddNumericRenderer(ev => ev.RecalculatedDistance).Digits(2)
				.AddColumn("Полезный\nпробег, %").AddTextRenderer(ev => ev.UsefulMileagePercent)
				.AddColumn("Факт расход").AddNumericRenderer(ev => ev.ConsumptionFact).Digits(2)
				.AddColumn("План расход").AddNumericRenderer(ev => ev.ConsumptionPlan).Digits(2)
				.AddColumn("Разница").AddNumericRenderer(ev => ev.DiscrepancyFuel).Digits(2)
				.AddColumn("Факт расход\nна 100км").AddNumericRenderer(ev => ev.Consumption100KmFact).Digits(2)
				.AddColumn("План расход\nна 100км").AddNumericRenderer(ev => ev.Consumption100KmPlan).Digits(2)
				.AddColumn("Разница в %").AddNumericRenderer(ev => ev.DiscrepancyPercent).Digits(2)
				.AddSetter((cell, node) =>
				{
					var discrepancyValue = node.DiscrepancyPercent;
					var discrepancyPercentFilter = ViewModel.DiscrepancyPercentFilter;

					cell.CellBackgroundGdk = node.IsSingleCalibrationForPeriod
						? GdkColors.BabyBlue
						: Math.Abs(discrepancyValue) <= discrepancyPercentFilter && Math.Abs(discrepancyValue) >= discrepancyPercentFilter * (-1)
							? GdkColors.SuccessBase
							: discrepancyValue < 0 && Math.Abs(discrepancyValue) > discrepancyPercentFilter
								? GdkColors.YellowMustard
								: GdkColors.DangerBase;
				})				
				.AddColumn("")
				.RowCells()
				.AddSetter<CellRenderer>(
					(cell, node) =>
					{
						cell.CellBackgroundGdk = node.IsSingleCalibrationForPeriod ? GdkColors.BabyBlue : GdkColors.PrimaryBase;
					})
				.Finish();

			ytreeviewReport.Binding.AddBinding(ViewModel, vm => vm.ReportRows, w => w.ItemsDataSource).InitializeFromSource();

			var carModelSelectionFilterView = new CarModelSelectionFilterView(ViewModel.CarModelSelectionFilterViewModel);
			yhboxCarModelContainer.Add(carModelSelectionFilterView);
			carModelSelectionFilterView.Show();

			buttonInfo.BindCommand(ViewModel.ShowHelpInfoCommand);
		}
	}
}
