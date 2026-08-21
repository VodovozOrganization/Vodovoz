using QS.Dialog;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.Application.Mango;
using Vodovoz.Presentation.ViewModels.Mango;

namespace Vodovoz.ViewModels.Dialogs.Mango.Talks
{
	public class TalkViewModelBase : WindowDialogViewModelBase, IDisposable
	{
		protected readonly MangoManager MangoManager;

		public TalkViewModelBase(INavigationManager navigation, MangoManager manager) : base(navigation)
		{
			MangoManager = manager ?? throw new ArgumentNullException(nameof(manager));
			ActiveCall = MangoManager.CurrentTalk ?? MangoManager.CurrentHold;
			MangoManager.PropertyChanged += Manager_PropertyChanged;
			SetTitle();
			IsModal = false;
			WindowPosition = WindowGravity.RightBottom;
			EnableMinimizeMaximize = true;
			//TODO MemoryLeaking
			ActiveCall.PropertyChanged += ActiveCall_PropertyChanged;
		}

		void Manager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(MangoManager.StageDuration))
				SetTitle();
		}

		void ActiveCall_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ActiveCall.CallState)) {
				OnPropertyChanged(nameof(PhoneText));
				OnPropertyChanged(nameof(CallerNameText));
				SetTitle();
			}
		}

		public ActiveCall ActiveCall { get; private set; }

		protected bool IsTalkOnHold => ActiveCall.CallState == CallState.OnHold;

		public string PhoneText => ActiveCall.CallerNumberText;

		public string CallerNameText => ActiveCall.CallerName;

		private void SetTitle()
		{
			Title = String.Format("{1} {0:mm\\:ss}", 
				MangoManager.StageDuration, 
				IsTalkOnHold ? "Удержание" : "Разговор");
		}

		public void FinishCallCommand()
		{
			MangoManager.HangUp();
			Close(true, CloseSource.Self);
		}

		public void ForwardCallCommand()
		{
			Action action = () => { Close(true, CloseSource.Self); };
			IPage page = NavigationManager.OpenViewModel<SubscriberSelectionViewModel, MangoManager, DialogType>
			(this, MangoManager, DialogType.Transfer);
		}

		public virtual void Dispose()
		{
			if(MangoManager != null)
			{
				MangoManager.PropertyChanged -= Manager_PropertyChanged;
			}
		}
	}
}
