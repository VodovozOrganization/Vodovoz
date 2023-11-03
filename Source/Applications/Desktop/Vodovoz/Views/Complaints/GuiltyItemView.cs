using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.Views.Complaints
{
	[ToolboxItem(true)]
	public partial class GuiltyItemView : WidgetViewBase<GuiltyItemViewModel>
	{
		public GuiltyItemView()
		{
			Build();
		}

		public GuiltyItemView(GuiltyItemViewModel viewModel) : base(viewModel)
		{
			Build();
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

			entrySubdivision.ViewModel = ViewModel.SubdivisionViewModel;
			entrySubdivision.Binding
				.AddBinding(ViewModel, vm => vm.CanChooseSubdivision, w => w.Visible)
				.InitializeFromSource();

			Shown += (s, ea) => {
				entVmEmployee.Visible = ViewModel.CanChooseEmployee;
			};
		}

		public override void Destroy()
		{
			yCmbResponsible.Destroy();
			base.Destroy();
		}
	}
}
