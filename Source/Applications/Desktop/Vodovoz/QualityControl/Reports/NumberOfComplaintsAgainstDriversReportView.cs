using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.ViewModels.QualityControl.Reports;
using static Vodovoz.ViewModels.QualityControl.Reports.NumberOfComplaintsAgainstDriversReportViewModel;

namespace Vodovoz.QualityControl.Reports
{
	[ToolboxItem(true)]
	public partial class NumberOfComplaintsAgainstDriversReportView : TabViewBase<NumberOfComplaintsAgainstDriversReportViewModel>
	{
		public NumberOfComplaintsAgainstDriversReportView(NumberOfComplaintsAgainstDriversReportViewModel viewModel) : base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			btnReportInfo.Visible = false;

			datePeriodPicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ybuttonCreateReport.Clicked += (s, e) => ViewModel.GenerateReportCommand.Execute();

			ybuttonSave.Clicked += (s, e) => ViewModel.ExportReportCommand.Execute();

			ybuttonSave.Binding
				.AddBinding(ViewModel, vm => vm.CanExportReport, w => w.Sensitive)
				.InitializeFromSource();

			ConfigureTreeView();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(NumberOfComplaintsAgainstDriversReportViewModel.Report))
			{
				ytreeReportIndicatorsRows.ItemsDataSource = ViewModel.Report.Rows;
			}
		}

		private void ConfigureTreeView()
		{
			ytreeReportIndicatorsRows.CreateFluentColumnsConfig<NumberOfComplaintsAgainstDriversReport.Row>()
				.AddColumn("ФИО").AddTextRenderer(x => x.DriverFullName)
				.AddColumn("Кол-во рекламаций").AddNumericRenderer(x => x.ComplaintsCount)
				.AddColumn("Номера рекламаций").AddTextRenderer(x => x.ComplaintsList)
				.AddColumn("")
				.Finish();
		}
	}
}
