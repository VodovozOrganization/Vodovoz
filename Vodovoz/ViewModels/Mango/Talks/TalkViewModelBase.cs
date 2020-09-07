using System;
using QS.Navigation;
using QS.ViewModels.Dialog;
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

	}
}
