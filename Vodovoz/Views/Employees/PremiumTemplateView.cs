using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.Views.Employees
{
	public partial class PremiumTemplateView : TabViewBase<PremiumTemplateViewModel>
	{
		public PremiumTemplateView(PremiumTemplateViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			yentryReason.Binding.AddBinding(ViewModel.Entity, e => e.Reason, w => w.Text).InitializeFromSource();
			yspinbuttonPremiumMoney.Binding.AddBinding(ViewModel.Entity, e => e.PremiumMoney, w => w.ValueAsDecimal).InitializeFromSource();

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
		}

	}
}
