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
			enumcomboCategory.Binding.AddBinding(ViewModel, vm => vm.CanChangeCategory, w => w.Sensitive).InitializeFromSource();

			yenumcomboStatus.ItemsEnum = typeof(EmployeeStatus);
			yenumcomboStatus.Binding.AddBinding(ViewModel, vm => vm.Status, w => w.SelectedItemOrNull).InitializeFromSource();
			yenumcomboStatus.Binding.AddBinding(ViewModel, vm => vm.CanChangeStatus, w => w.Sensitive).InitializeFromSource();

			hboxDriversAndTerminals.Binding.AddBinding(ViewModel, vm => vm.HasAccessToDriverTerminal, w => w.Visible).InitializeFromSource();
			if(ViewModel.HasAccessToDriverTerminal)
			{
				comboDriverType.ItemsEnum = typeof(DriverTerminalRelation);
				comboDriverType.Binding.AddBinding(ViewModel, vm => vm.DriverTerminalRelation, w => w.SelectedItemOrNull).InitializeFromSource();

				checkSortByPriority.Binding.AddBinding(ViewModel, vm => vm.SortByPriority, w => w.Active);
				checkSortByPriority.Toggled += (sender, args) => ViewModel.UpdateRestrictions.Execute();
			}
		}
	}
}
