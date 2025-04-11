using QS.Commands;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public class TextEditViewModel : WindowDialogViewModelBase
	{
		private string _oldText = string.Empty;
		private string _newText = string.Empty;
		private bool _showOldText;

		public TextEditViewModel(INavigationManager navigation)
			: base(navigation)
		{
			WindowPosition = QS.Dialog.WindowGravity.None;
			IsModal = true;

			AcceptCommand = new DelegateCommand(Accept);
			CancelCommand = new DelegateCommand(Cancel);
		}

		public event EventHandler<TextChangeEventArgs> TextChanged;

		public string OldText
		{
			get => _oldText;
			set => SetField(ref _oldText, value);
		}

		public string NewText
		{
			get => _newText;
			set => SetField(ref _newText, value);
		}

		public bool ShowOldText
		{
			get => _showOldText;
			set => SetField(ref _showOldText, value);
		}

		public DelegateCommand AcceptCommand { get; }
		public DelegateCommand CancelCommand { get; }

		public void Configure(Action<TextEditViewModel> action)
		{
			action?.Invoke(this);
		}

		private void Accept()
		{
			TextChanged?.Invoke(this, new TextChangeEventArgs(OldText, NewText));
			Close(false, CloseSource.Save);
		}

		private void Cancel()
		{
			Close(false, CloseSource.Self);
		}
	}
}
