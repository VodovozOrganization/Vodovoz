using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.ViewModels.QualityControl.Reports;

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
			ybuttonCreateReport.Clicked += (s, e) => ViewModel.GenerateReportCommand.Execute();

			ybuttonSave.Clicked += (s, e) => ViewModel.ExportReportCommand.Execute();

			ybuttonSave.Binding
				.AddBinding(ViewModel, vm => vm.CanExportReport, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
