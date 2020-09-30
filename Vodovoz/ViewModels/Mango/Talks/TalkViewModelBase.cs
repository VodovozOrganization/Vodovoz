using System;
using System.Collections.Generic;
using QS.Dialog;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Contacts;
using Vodovoz.Infrastructure.Mango;

namespace Vodovoz.ViewModels.Mango.Talks
{
	public class TalkViewModelBase : WindowDialogViewModelBase
	{
		public readonly Phone Phone; 
		protected readonly MangoManager MangoManager;

		public TalkViewModelBase(INavigationManager navigation, MangoManager manager) : base(navigation)
		{
			this.MangoManager = manager ?? throw new ArgumentNullException(nameof(manager));
			manager.PropertyChanged += Manager_PropertyChanged;
			SetTitle();
			IsModal = false;
			WindowPosition = WindowGravity.RightBottom;
			Phone = manager.Phone;
		}

		void Manager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(MangoManager.StageDuration))
				SetTitle();
		}

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
			IPage page = NavigationManager.OpenViewModelNamedArgs<SubscriberSelectionViewModel>
			(this, new Dictionary<string, object>()
				{ {"manager",MangoManager },{"dialogType", SubscriberSelectionViewModel.DialogType.AdditionalCall },
				{"exitAction", action}
			});
		}

	}
}
