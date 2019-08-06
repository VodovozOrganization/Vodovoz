using Gamma.Binding.Core;
using QS.Views.GtkUI;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.Views.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GuiltyItemView : EntityWidgetViewBase<GuiltyItemViewModel>
	{
		public BindingControler<GuiltyItemView> Binding { get; private set; }

		public GuiltyItemView()
		{
			this.Build();
			Binding = new BindingControler<GuiltyItemView>(this);
		}

		public GuiltyItemView(GuiltyItemViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Binding = new BindingControler<GuiltyItemView>(this);
		}

		protected override void ConfigureWidget()
		{
			if(ViewModel != null) {
				yEnumGuiltyType.ItemsEnum = typeof(ComplaintGuiltyTypes);
				yEnumGuiltyType.Binding.AddBinding(ViewModel.Entity, s => s.GuiltyType, w => w.SelectedItemOrNull).InitializeFromSource();

				entVmEmployee.Binding.AddBinding(ViewModel.Entity, e => e.Employee, w => w.Subject).InitializeFromSource();
				entVmEmployee.Binding.AddBinding(ViewModel, vm => vm.CanChooseEmployee, w => w.Visible).InitializeFromSource();

				yCmbSubdivision.Binding.AddBinding(ViewModel, s => s.AllDepartments, w => w.ItemsList).InitializeFromSource();
				yCmbSubdivision.Binding.AddBinding(ViewModel.Entity, s => s.Subdivision, w => w.SelectedItem).InitializeFromSource();
				yCmbSubdivision.Binding.AddBinding(ViewModel, vm => vm.CanChooseSubdivision, w => w.Visible).InitializeFromSource();
			}
		}
	}
}
