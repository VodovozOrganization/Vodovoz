using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Employees;
namespace Vodovoz.Views.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FineTemplateView : TabViewBase<FineTemplateViewModel>
	{
		public FineTemplateView(FineTemplateViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryReason.Binding.AddBinding(ViewModel.Entity, e => e.Reason, w => w.Text).InitializeFromSource();
			yentryReason.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			yspinbuttonFineMoney.Digits = 2;
			yspinbuttonFineMoney.Binding.AddBinding(ViewModel.Entity, e => e.FineMoney, w => w.ValueAsDecimal).InitializeFromSource();
			yspinbuttonFineMoney.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
		}
	}
}
