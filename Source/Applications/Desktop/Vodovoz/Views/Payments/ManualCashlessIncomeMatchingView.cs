using QS.Navigation;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.ViewModels.Payments;

namespace Vodovoz.Views
{
	[ToolboxItem(true)]
	public partial class ManualCashlessIncomeMatchingView : TabViewBase<ManualCashlessIncomeMatchingViewModel>
	{
		public ManualCashlessIncomeMatchingView(ManualCashlessIncomeMatchingViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		void Configure()
		{
			btnSave.Clicked += (sender, args) => ViewModel.SaveViewModelCommand.Execute();
			btnCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
			btnCompleteAllocation.Clicked += (sender, args) => ViewModel.CompleteAllocationCommand.Execute();


			lblIncomeSum.Text = ViewModel.Entity.Total.ToString();
			lblLastBalance.Binding
				.AddBinding(ViewModel, vm => vm.LastBalance, w => w.Text, new DecimalToStringConverter())
				.InitializeFromSource();
			lblToAllocate.Binding
				.AddBinding(ViewModel, vm => vm.SumToAllocate, w => w.Text, new DecimalToStringConverter())
				.InitializeFromSource();

			lblPayer.Text = ViewModel.Entity.CounterpartyName;
			lblIncomeNumber.Text = ViewModel.Entity.PaymentNum.ToString();
			lblDate.Text = ViewModel.Entity.Date.ToShortDateString();

			textViewPurpose.Binding
				.AddSource(ViewModel.Entity)
				.AddBinding(e => e.PaymentPurpose, w => w.Buffer.Text)
				.AddBinding(e => e.IsManuallyCreated, w => w.Editable)
				.InitializeFromSource();
		}
	}
}
