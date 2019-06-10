using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;

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
			enumcomboCategory.ItemsEnum = typeof(EmployeeCategory);
			enumcomboCategory.ShowSpecialStateAll = true;
			enumcomboCategory.ShowSpecialStateNot = false;

			enumcomboCategory.Binding.AddBinding(ViewModel, vm => vm.Category, w => w.SelectedItemOrNull).InitializeFromSource();
			enumcomboCategory.Binding.AddFuncBinding(ViewModel, vm => vm.IsCategoryNotRestricted, w => w.Sensitive).InitializeFromSource();
			checkFired.Binding.AddBinding(ViewModel, vm => vm.ShowFired, w => w.Active).InitializeFromSource();
		}
	}
}
