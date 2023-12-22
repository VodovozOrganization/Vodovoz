using QS.Views.Dialog;
using System;
using Vodovoz.Presentation.ViewModels.Pacs;
namespace Vodovoz.Views.Pacs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PacsOperatorReferenceBookView : DialogViewBase<PacsOperatorReferenceBookViewModel>
	{
		public PacsOperatorReferenceBookView(PacsOperatorReferenceBookViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			entityentryEmployee.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeOperator, w => w.Sensitive)
				.AddBinding(vm => vm.OperatorEntry, w => w.ViewModel)
				.InitializeFromSource();

			entityentryWorkshift.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.WorkShiftEntry, w => w.ViewModel)
				.InitializeFromSource();

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CancelCommand);
		}
	}
}
