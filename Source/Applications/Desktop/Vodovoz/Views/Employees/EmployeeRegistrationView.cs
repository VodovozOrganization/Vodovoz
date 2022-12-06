using QS.Navigation;
using QS.Views;
using Vodovoz.ViewModels.Employees;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Views.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeRegistrationView : ViewBase<EmployeeRegistrationViewModel>
	{
		public EmployeeRegistrationView(EmployeeRegistrationViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			btnSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			btnCancel.Clicked += (sender, e) => ViewModel.Close(false, CloseSource.Cancel);

			enumRegistrationType.ItemsEnum = typeof(RegistrationType);
			enumRegistrationType.Binding
				.AddBinding(ViewModel.Entity, e => e.RegistrationType, w => w.SelectedItem)
				.InitializeFromSource();
			enumPaymentForm.ItemsEnum = typeof(PaymentForm);
			enumPaymentForm.Binding
				.AddBinding(ViewModel.Entity, e => e.PaymentForm, w => w.SelectedItem)
				.InitializeFromSource();

			spinTaxRate.Binding
				.AddBinding(ViewModel.Entity, e => e.TaxRate, w => w.ValueAsDecimal)
				.InitializeFromSource();
		}
	}
}
