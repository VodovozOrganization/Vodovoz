using System;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Utilities.Numeric;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure.Mango;

namespace Vodovoz.ViewModels.Mango
{
	public class IncomingCallViewModel : WindowDialogViewModelBase
	{
		public readonly MangoManager MangoManager;

		public IncomingCallViewModel(INavigationManager navigation,
			MangoManager manager) : base(navigation)
		{
			this.MangoManager = manager ?? throw new ArgumentNullException(nameof(manager));
			if(manager.IsTransfer && manager.PrimaryCaller != null) {
				string number;
				if(MangoManager.PrimaryCaller.Number.Length == 11) {
					var formatter = new PhoneFormatter(PhoneFormat.BracketWithWhitespaceLastTen);
					number = "+7 " + formatter.FormatString(manager.PrimaryCaller.Number);
				} else number = manager.PrimaryCaller.Number;
				OnLineText = $"{number}\n{MangoManager.PrimaryCallerNames}".TrimEnd();
			}
			IsModal = false;
			WindowPosition = WindowGravity.RightBottom;
			if(MangoManager.IsOutgoing)
				Title = "Исходящий звонок";
			else
				Title = "Входящий звонок";
		}

		#region Свойства View

		public string OnLineText { get; private set; }
		public bool ShowTransferCaller => MangoManager.IsTransfer;

		#endregion

		#region Действия View

		public void DeclineCall()
		{
			MangoManager.HangUp();
			Close(false, CloseSource.Self);
		}

		#endregion
	}
}
