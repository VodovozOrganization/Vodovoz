using QS.Views.Dialog;
using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.Presentation.Views.Common
{
	public partial class TextEditView : DialogViewBase<TextEditViewModel>
	{
		public TextEditView(TextEditViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			yentryOldValue.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.OldText, w => w.Text)
				.AddBinding(vm => vm.ShowOldText, w => w.Visible)
				.InitializeFromSource();

			yentryNewValue.Binding
				.AddBinding(ViewModel, vm => vm.NewText, w => w.Text)
				.InitializeFromSource();

			ybuttonAccept.BindCommand(ViewModel.AcceptCommand);
			ybuttonCancel.BindCommand(ViewModel.CancelCommand);
		}
	}
}
