using QS.Views.Dialog;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.Pacs;
namespace Vodovoz.Views.Pacs
{
	[ToolboxItem(true)]
	public partial class PacsOperatorReferenceBookView : DialogViewBase<PacsOperatorReferenceBookViewModel>
	{
		public PacsOperatorReferenceBookView(PacsOperatorReferenceBookViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			entityentryEmployee.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeEmployee, w => w.Sensitive)
				.AddBinding(vm => vm.OperatorEntry, w => w.ViewModel)
				.InitializeFromSource();

			entityentryWorkshift.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.WorkShiftEntry, w => w.ViewModel)
				.InitializeFromSource();

			pacsEnabled.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.PacsEnabled, w => w.Active)
				.InitializeFromSource();

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CancelCommand);
		}
	}
}
