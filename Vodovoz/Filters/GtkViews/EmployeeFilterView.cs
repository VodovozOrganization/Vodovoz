using QS.Views.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeFilterView : FilterViewBase<EmployeeFilterViewModel>
	{
		public EmployeeFilterView(EmployeeFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			enumcomboCategory.ItemsEnum = typeof(EmployeeCategory);
			enumcomboCategory.ShowSpecialStateAll = true;
			enumcomboCategory.ShowSpecialStateNot = false;

			enumcomboCategory.Binding.AddBinding(ViewModel, vm => vm.Category, w => w.SelectedItemOrNull).InitializeFromSource();
			enumcomboCategory.Binding.AddFuncBinding(ViewModel, vm => vm.IsCategoryNotRestricted, w => w.Sensitive).InitializeFromSource();

			yenumcomboStatus.ItemsEnum = typeof(EmployeeStatus);
			yenumcomboStatus.Binding.AddBinding(ViewModel, vm => vm.Status, w => w.SelectedItemOrNull).InitializeFromSource();
			yenumcomboStatus.Binding.AddFuncBinding(ViewModel, vm => vm.CanChangeStatus, w => w.Sensitive).InitializeFromSource();
		}
	}
}
