using QS.Navigation;
using Vodovoz.Infrastructure.Mango;

namespace Vodovoz.ViewModels.Mango.Talks
{
	public class InternalTalkViewModel : TalkViewModelBase
	{
		public InternalTalkViewModel(
			ITdiCompatibilityNavigation navigation,
			MangoManager manager) : base(navigation, manager)
		{
			
		}

		#region Свойства View

		public string OnLineText => MangoManager.CurrentTalk?.OnHoldText;
		public bool ShowTransferCaller => MangoManager.CurrentTalk?.IsTransfer ?? false;
		public bool ShowReturnButton => (MangoManager.CurrentTalk?.IsTransfer ?? false) && MangoManager.IsOutgoing;
		public bool ShowTransferButton => !MangoManager.CurrentTalk?.IsTransfer ?? true;

		#endregion

		#region Действия View

		public string GetCallerName()
		{
			return MangoManager.CallerName;
		}
		#endregion
	}
}
