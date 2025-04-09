using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.Presentation.Views.Common
{
	public partial class TextEditView : Gtk.Dialog
	{
		private readonly TextEditViewModel _viewModel;

		public TextEditView(TextEditViewModel viewModel)
		{
			_viewModel = viewModel
				?? throw new System.ArgumentNullException(nameof(viewModel));

			WindowPosition = Gtk.WindowPosition.None;

			Build();

			Initialize();
		}

		private void Initialize()
		{
			yentryOldValue.Binding
				.AddSource(_viewModel)
				.AddBinding(vm => vm.OldText, w => w.Text)
				.AddBinding(vm => vm.ShowOldText, w => w.Visible)
				.InitializeFromSource();

			yentryNewValue.Binding
				.AddBinding(_viewModel, vm => vm.NewText, w => w.Text)
				.InitializeFromSource();

			ybuttonAccept.BindCommand(_viewModel.AcceptCommand);
			ybuttonCancel.BindCommand(_viewModel.CancelCommand);
		}
	}
}
