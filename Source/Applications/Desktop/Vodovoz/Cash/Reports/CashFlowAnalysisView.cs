using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Cash.Reports;

namespace Vodovoz.Cash.Reports
{
	public partial class CashFlowAnalysisView : TabViewBase<CashFlowAnalysisViewModel>
	{
		public CashFlowAnalysisView(CashFlowAnalysisViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			UpdateSliderArrow();

			dateStart.Binding
				.AddBinding(ViewModel, vm => vm.StartDate, w => w.Date)
				.InitializeFromSource();

			dateEnd.Binding
				.AddBinding(ViewModel, vm => vm.EndDate, w => w.Date)
				.InitializeFromSource();

			ybuttonCreateReport.Binding
				.AddBinding(ViewModel, vm => vm.CanGenerateDdsReport, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonCreateReport.Clicked += (s, e) => ViewModel.GenerateDdsReportCommand.Execute();

			ybuttonSave.Binding
				.AddBinding(ViewModel, vm => vm.CanSaveReport, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonSave.Clicked += (s, e) => ViewModel.SaveReportCommand.Execute();

			buttonInfo.Clicked += (s, e) => ViewModel.ShowDdsReportInfoCommand.Execute();

			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = vboxParameters.Visible ? ArrowType.Left : ArrowType.Right;
		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			vboxParameters.Visible = !vboxParameters.Visible;
			UpdateSliderArrow();
		}
	}
}
