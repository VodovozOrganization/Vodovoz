using System;
using QS.Dialog;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Contacts;
using Vodovoz.Infrastructure.Mango;

namespace Vodovoz.ViewModels.Mango.Talks
{
	public class TalkViewModelBase : WindowDialogViewModelBase
	{
		protected readonly MangoManager MangoManager;

		public TalkViewModelBase(INavigationManager navigation, MangoManager manager) : base(navigation)
		{
			this.MangoManager = manager ?? throw new ArgumentNullException(nameof(manager));
			manager.PropertyChanged += Manager_PropertyChanged;
			SetTitle();
			IsModal = false;
			WindowPosition = WindowGravity.RightBottom;
			EnableMinimizeMaximize = true;
		}

		void Manager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(MangoManager.StageDuration))
				SetTitle();
		}

		public Phone Phone => MangoManager.Phone;

		private void SetTitle()
		{
			Title = String.Format("Разговор {0:mm\\:ss}", MangoManager.StageDuration);
		}

		public void FinishCallCommand()
		{
			MangoManager.HangUp();
			Close(false, CloseSource.Self);
		}

		public void ForwardCallCommand()
		{
			Action action = () => { Close(false, CloseSource.Self); };
			IPage page = NavigationManager.OpenViewModel<SubscriberSelectionViewModel, MangoManager, SubscriberSelectionViewModel.DialogType>
			(this, MangoManager, SubscriberSelectionViewModel.DialogType.AdditionalCall);
		}

	}
}
