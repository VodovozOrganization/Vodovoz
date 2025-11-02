using QS.Dialog;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;

namespace Vodovoz.ViewModels.Dialogs.Mango
{
	public class IncomingCallViewModel : WindowDialogViewModelBase
	{
		public readonly MangoManager MangoManager;

		public IncomingCallViewModel(INavigationManager navigation,
			MangoManager manager) : base(navigation)
		{
			this.MangoManager = manager ?? throw new ArgumentNullException(nameof(manager));
			IsModal = false;
			WindowPosition = WindowGravity.RightBottom;
			if(MangoManager.IsOutgoing)
				Title = "Исходящий звонок";
			else
				Title = "Входящий звонок";
		}

		#region Действия View

		public void DeclineCall()
		{
			MangoManager.HangUp();
			Close(true, CloseSource.Self);
		}

		#endregion
	}
}
