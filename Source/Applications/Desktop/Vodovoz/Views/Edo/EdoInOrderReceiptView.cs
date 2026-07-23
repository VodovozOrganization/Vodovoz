using QS.Views.GtkUI;
using Vodovoz.ViewModels.Edo;
namespace Vodovoz.Views.Edo
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoInOrderReceiptView : WidgetViewBase<EdoInOrderReceiptViewModel>
	{
		public EdoInOrderReceiptView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			yentryDocGuid.Binding
				.AddBinding(ViewModel, vm => vm.DocGuid, w => w.Text)
				.InitializeFromSource();

			yentryDocNumber.Binding
				.AddBinding(ViewModel, vm => vm.DocNumber, w => w.Text)
				.InitializeFromSource();

			yentryDocType.Binding
				.AddBinding(ViewModel, vm => vm.DocType, w => w.Text)
				.InitializeFromSource();

			yentryDocTime.Binding
				.AddBinding(ViewModel, vm => vm.DocTime, w => w.Text)
				.InitializeFromSource();

			yentryDocStatus.Binding
				.AddBinding(ViewModel, vm => vm.DocStatus, w => w.Text)
				.InitializeFromSource();

			yentryDocIndex.Binding
				.AddBinding(ViewModel, vm => vm.DocIndex, w => w.Text)
				.InitializeFromSource();

			yentryContact.Binding
				.AddBinding(ViewModel, vm => vm.Contact, w => w.Text)
				.InitializeFromSource();

			yentryFiscalNumber.Binding
				.AddBinding(ViewModel, vm => vm.FiscalNumber, w => w.Text)
				.InitializeFromSource();

			yentryFiscalKktNumber.Binding
				.AddBinding(ViewModel, vm => vm.FiscalKktNumber, w => w.Text)
				.InitializeFromSource();

			yentryFiscalMark.Binding
				.AddBinding(ViewModel, vm => vm.FiscalMark, w => w.Text)
				.InitializeFromSource();

			yentryFiscalTime.Binding
				.AddBinding(ViewModel, vm => vm.FiscalTime, w => w.Text)
				.InitializeFromSource();

			yentryCashier.Binding
				.AddBinding(ViewModel, vm => vm.Cashier, w => w.Text)
				.InitializeFromSource();

			yentryResaleInn.Binding
				.AddBinding(ViewModel, vm => vm.ResaleInn, w => w.Text)
				.InitializeFromSource();

			yentrySum.Binding
				.AddBinding(ViewModel, vm => vm.Sum, w => w.Text)
				.InitializeFromSource();

			ylabelCashError.Binding
				.AddBinding(ViewModel, vm => vm.HasCashError, w => w.Visible)
				.InitializeFromSource();
			ytextviewCashError.Binding
				.AddBinding(ViewModel, vm => vm.CashError, w => w.Buffer.Text)
				.AddBinding(ViewModel, vm => vm.HasCashError, w => w.Visible)
				.InitializeFromSource();
			scrollWindowCashError.Visible = ViewModel.HasCashError;

			ViewModel.PropertyChanged += ViewModelPropertyChanged;
		}

		private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(EdoInOrderReceiptViewModel.HasCashError))
			{
				scrollWindowCashError.Visible = ViewModel.HasCashError;
			}
		}
	}
}
