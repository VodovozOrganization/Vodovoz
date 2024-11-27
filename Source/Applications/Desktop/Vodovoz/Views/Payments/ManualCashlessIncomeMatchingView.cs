using System;
using QS.Navigation;
using QS.Views.GtkUI;
using System.ComponentModel;
using Gtk;
using Vodovoz.Core;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.ViewModels.Payments;

namespace Vodovoz.Views
{
	[ToolboxItem(true)]
	public partial class ManualCashlessIncomeMatchingView : TabViewBase<ManualCashlessIncomeMatchingViewModel>
	{
		private IWidgetResolver _widgetResolver = ViewModelWidgetResolver.Instance;
		
		public ManualCashlessIncomeMatchingView(ManualCashlessIncomeMatchingViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnSave.Clicked += (sender, args) => ViewModel.SaveViewModelCommand.Execute();
			btnCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
			btnCompleteAllocation.Clicked += (sender, args) => ViewModel.CompleteAllocationCommand.Execute();
			btnAddPayment.Clicked += OnAddPaymentClicked;
			
			lblIncomeSum.Text = ViewModel.Entity.Total.ToString();

			lblPayer.Text = ViewModel.Entity.PayerName;
			lblIncomeNumber.Text = ViewModel.Entity.Number.ToString();
			lblDate.Text = ViewModel.Entity.Date.ToShortDateString();

			textViewPurpose.Binding
				.AddSource(ViewModel.Entity)
				.AddBinding(e => e.PaymentPurpose, w => w.Buffer.Text)
				.AddBinding(e => e.IsManuallyCreated, w => w.Editable)
				.InitializeFromSource();

			CreatePaymentsViews();
		}

		private void OnAddPaymentClicked(object sender, EventArgs e)
		{
			if(ViewModel.AddNewPayment(out var paymentViewModel))
			{
				AddPaymentTab(paymentViewModel);
			}
		}

		private void CreatePaymentsViews()
		{
			foreach(var paymentViewModel in ViewModel.PaymentsViewModels)
			{
				AddPaymentTab(paymentViewModel);
			}
		}

		private void AddPaymentTab(ManualPaymentMatchingViewModel paymentViewModel)
		{
			var widget = CreatePaymentView(paymentViewModel);
			notebookPayments.Add(widget);
		}

		private Widget CreatePaymentView(ManualPaymentMatchingViewModel paymentViewModel)
		{
			var paymentWidgetView = _widgetResolver.Resolve(paymentViewModel);
			paymentWidgetView.Show();

			return paymentWidgetView;
		}
	}
}
