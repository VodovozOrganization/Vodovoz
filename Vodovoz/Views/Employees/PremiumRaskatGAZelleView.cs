using System.Linq;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.Views.Employees
{
	public partial class PremiumRaskatGAZelleView : TabViewBase<PremiumRaskatGAZelleViewModel>
	{
		public PremiumRaskatGAZelleView(PremiumRaskatGAZelleViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ylabelDate.Binding.AddFuncBinding(ViewModel.Entity, e => e.Date.ToString("D"), w => w.LabelProp).InitializeFromSource();
			ylabelMoney.Binding.AddFuncBinding(ViewModel.Entity, e => e.TotalMoney.ToString("C"), w => w.LabelProp).InitializeFromSource();
			ylabelReason.Binding.AddBinding(ViewModel.Entity, e => e.PremiumReasonString, w => w.LabelProp).InitializeFromSource();
			ylabelAuthor.Binding.AddFuncBinding(ViewModel.Entity, e => e.Author.FullName, w => w.LabelProp).InitializeFromSource();
			ylabelPremiumEmployee.Binding.AddFuncBinding(ViewModel, vm => vm.EmployeeFullName,
				w => w.LabelProp).InitializeFromSource();
			
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
		}
	}
}
