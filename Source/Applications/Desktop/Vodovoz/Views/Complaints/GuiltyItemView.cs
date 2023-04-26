using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
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
			yCmbResponsible.SetRenderTextFunc<Responsible>(r => r.Name);
			yCmbResponsible.Binding
				.AddBinding(ViewModel, vm => vm.ResponsibleList, w => w.ItemsList)
				.AddBinding(ViewModel.Entity, e => e.Responsible, w => w.SelectedItem)
				.InitializeFromSource();

			entVmEmployee.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeSelectorFactory);
			entVmEmployee.Binding.AddBinding(ViewModel.Entity, e => e.Employee, w => w.Subject).InitializeFromSource();
			entVmEmployee.Binding.AddBinding(ViewModel, vm => vm.CanChooseEmployee, w => w.Visible).InitializeFromSource();
			entVmEmployee.CanOpenWithoutTabParent = true;

			entityentrySubdivision.SetEntityAutocompleteSelectorFactory(ViewModel.SubdivisionSelectorFactory);
			entityentrySubdivision.Binding
				.AddBinding(ViewModel.Entity, s => s.Subdivision, w => w.Subject)
				.AddBinding(ViewModel, vm => vm.CanChooseSubdivision, w => w.Visible)
				.InitializeFromSource();
			entityentrySubdivision.CanOpenWithoutTabParent = true;

			this.Shown += (s, ea) => {
				entVmEmployee.Visible = ViewModel.CanChooseEmployee;
				entityentrySubdivision.Visible = ViewModel.CanChooseSubdivision;
			};
		}

		public override void Destroy()
		{
			yCmbResponsible.Destroy();
			entityentrySubdivision.Destroy();
			base.Destroy();
		}
	}
}
