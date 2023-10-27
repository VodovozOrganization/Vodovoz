using QS.Views.GtkUI;
using Vodovoz.ViewModels.Counterparties.ClientClassification;

namespace Vodovoz.Views.Client.CounterpartyClassification
{
	public partial class CounterpartyClassificationCalculationView : TabViewBase<CounterpartyClassificationCalculationViewModel>
	{
		public CounterpartyClassificationCalculationView(CounterpartyClassificationCalculationViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yspinbuttonPeriod.Binding
				.AddBinding(ViewModel.CalculationSettings, s => s.PeriodInMonths, w => w.ValueAsInt)
				.InitializeFromSource();

			yspinbuttonABottlesCount.Binding
				.AddBinding(ViewModel.CalculationSettings, s => s.BottlesCountAClassificationFrom, w => w.ValueAsInt)
				.InitializeFromSource();

			yspinbuttonCBottlesCount.Binding
				.AddBinding(ViewModel.CalculationSettings, s => s.BottlesCountCClassificationTo, w => w.ValueAsInt)
				.InitializeFromSource();

			yspinbuttonXOrdersCount.Binding
				.AddBinding(ViewModel.CalculationSettings, s => s.OrdersCountXClassificationFrom, w => w.ValueAsInt)
				.InitializeFromSource();

			yspinbuttonZOrdersCount.Binding
				.AddBinding(ViewModel.CalculationSettings, s => s.OrdersCountZClassificationTo, w => w.ValueAsInt)
				.InitializeFromSource();

			ybuttonCalculate.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanOpenEmailSettingsDialog, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonCalculate.Clicked += (s, e) => ViewModel.OpenEmailSettingsDialogCommand.Execute();

			ybuttonCancel.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanCancel, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonCancel.Clicked += (s, e) => ViewModel.CancelCommand.Execute();

			ybuttonSaveXls.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanSaveReport, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonSaveXls.Clicked += (s, e) => ViewModel.SaveReportCommand.Execute();

			ybuttonQuite.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanQuite, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonQuite.Clicked += (s, e) => ViewModel.QuiteCommand.Execute();
		}
	}
}
