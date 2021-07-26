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

			enumcomboCategory.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Category, w => w.SelectedItemOrNull)
				.AddBinding(vm => vm.IsCategoryNotRestricted, w => w.Sensitive)
				.InitializeFromSource();

			yenumcomboStatus.ItemsEnum = typeof(EmployeeStatus);
			yenumcomboStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Status, w => w.SelectedItemOrNull)
				.AddBinding(vm => vm.CanChangeStatus, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
