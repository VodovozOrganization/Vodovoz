using System;
using Vodovoz.Filters.ViewModels;
using Gamma.Widgets;
using Vodovoz.Domain.Employees;
namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeFilterView : FilterViewBase<EmployeeFilterViewModel>
	{
		public EmployeeFilterView(EmployeeFilterViewModel viewModel)
		{
			this.Build();
			ViewModel = viewModel;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			enumcomboCategory.ShowSpecialStateAll = true;
			enumcomboCategory.ShowSpecialStateNot = false;

			enumcomboCategory.Binding.AddBinding(ViewModel, vm => vm.Category, w => w.SelectedItemOrNull).InitializeFromSource();
			enumcomboCategory.Binding.AddFuncBinding(ViewModel, vm => vm.CategoryRestricted, w => w.Sensitive).InitializeFromSource();
			checkFired.Binding.AddBinding(ViewModel, vm => vm.ShowFired, w => w.Active).InitializeFromSource();
		}
	}
}
