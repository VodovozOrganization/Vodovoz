using QS.Commands;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace Vodovoz.ViewModels
{
	public class AboutAuthorsViewModel : WindowDialogViewModelBase
	{
		public AboutAuthorsViewModel(INavigationManager navigation) : base(navigation)
		{
			WindowPosition = QS.Dialog.WindowGravity.None;
			Resizable = false;

			Title = "Авторы";

			CloseCommand = new DelegateCommand(Close);
		}

		public string Authors { get; set; }

		public DelegateCommand CloseCommand { get; }

		private void Close()
		{
			Close(false, CloseSource.Cancel);
		}
	}
}
