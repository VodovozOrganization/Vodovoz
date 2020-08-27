using System;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Infrastructure.Mango;

namespace Vodovoz.ViewModels.Mango
{
	public class IncomingCallViewModel : ModalDialogViewModelBase
	{
		public readonly MangoManager MangoManager;

		public IncomingCallViewModel(INavigationManager navigation, MangoManager manager) : base(navigation)
		{
			this.MangoManager = manager ?? throw new ArgumentNullException(nameof(manager));
			manager.PropertyChanged += Manager_PropertyChanged;
			SetTitle();
		}

		#region Действия View

		public void DeclineCall()
		{
			//FIXME Реализовать команду
		}

		#endregion

		void Manager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(MangoManager.StageDuration))
				SetTitle();
		}

		private void SetTitle()
		{
			Title = String.Format("Входящий звонок {0:mm\\:ss}", MangoManager.StageDuration);
		}

	}
}
