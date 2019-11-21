using QS.Views.GtkUI;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.Views.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GuiltyItemView : WidgetViewBase<GuiltyItemViewModel>
	{
		public GuiltyItemView()
		{
			this.Build();
		}

		public GuiltyItemView(GuiltyItemViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			yEnumGuiltyType.ItemsEnum = typeof(ComplaintGuiltyTypes);
			yEnumGuiltyType.Binding.AddBinding(ViewModel.Entity, s => s.GuiltyType, w => w.SelectedItemOrNull).InitializeFromSource();
			entVmEmployee.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeSelectorFactory);
			entVmEmployee.Binding.AddBinding(ViewModel.Entity, e => e.Employee, w => w.Subject).InitializeFromSource();
			entVmEmployee.Binding.AddBinding(ViewModel, vm => vm.CanChooseEmployee, w => w.Visible).InitializeFromSource();
			yCmbSubdivision.Binding.AddBinding(ViewModel, s => s.AllDepartments, w => w.ItemsList).InitializeFromSource();
			yCmbSubdivision.Binding.AddBinding(ViewModel.Entity, s => s.Subdivision, w => w.SelectedItem).InitializeFromSource();
			yCmbSubdivision.Binding.AddBinding(ViewModel, vm => vm.CanChooseSubdivision, w => w.Visible).InitializeFromSource();

			this.Shown += (s, ea) => {
				entVmEmployee.Visible = ViewModel.CanChooseEmployee;
				yCmbSubdivision.Visible = ViewModel.CanChooseSubdivision;
			};
		}
	}
}