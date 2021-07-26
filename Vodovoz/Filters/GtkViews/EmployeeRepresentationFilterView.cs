using QS.Views.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.JournalFilters;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeRepresentationFilterView : FilterViewBase<EmployeeRepresentationFilterViewModel>
	{
		public EmployeeRepresentationFilterView(EmployeeRepresentationFilterViewModel viewModel) : base(viewModel)
		{
			Build();
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
