using QS.Views.Dialog;
using System;
using Vodovoz.Presentation.ViewModels.Employees;

namespace Vodovoz.Views.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class InnerPhoneView : DialogViewBase<InnerPhoneViewModel>
	{
		public InnerPhoneView(InnerPhoneViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			entryNumber.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Number, w => w.Text)
				.AddBinding(vm => vm.CanChangeNumber, w => w.Sensitive)
				.InitializeFromSource();

			entryDescription.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Description, w => w.Text)
				.InitializeFromSource();

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CancelCommand);
		}
	}
}
