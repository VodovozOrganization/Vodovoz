using System;
using System.Collections.Generic;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Utilities.Numeric;
using Vodovoz.Infrastructure.Mango;

namespace Vodovoz.ViewModels.Mango.Talks
{
	public class InternalTalkViewModel : TalkViewModelBase
	{
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly IUnitOfWork UoW;
		private readonly ITdiCompatibilityNavigation tdiCompatibilityNavigation;
		private readonly IInteractiveQuestion interactive;


		public InternalTalkViewModel(IUnitOfWorkFactory unitOfWorkFactory,
			ITdiCompatibilityNavigation navigation,
			IInteractiveQuestion interactive,
			MangoManager manager) : base(navigation,manager)
		{
			this.unitOfWorkFactory = unitOfWorkFactory;
			this.tdiCompatibilityNavigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.UoW = unitOfWorkFactory.CreateWithoutRoot();
			if(manager.IsTransfer && manager.PrimaryCaller != null) {
				string number;
				if(MangoManager.PrimaryCaller.Number.Length == 11) {
					var formatter = new PhoneFormatter(PhoneFormat.BracketWithWhitespaceLastTen);
					number = "+7 " + formatter.FormatString(manager.PrimaryCaller.Number);
				} else number = manager.PrimaryCaller.Number;
				OnLineText = $"{number}\n{MangoManager.PrimaryCallerNames}".TrimEnd();
			}

			if(manager.IsOutgoing)
				Title = "Исходящий звонок";
			else
				Title = "Входящий звонок";
		}

		#region Свойства View

		public string OnLineText { get; private set; }
		public bool ShowTransferCaller => MangoManager.IsTransfer;
		public bool ShowReturnButton => MangoManager.IsTransfer && MangoManager.IsOutgoing;
		public bool ShowTransferButton => !MangoManager.IsTransfer;

		#endregion

		#region Действия View
		public string GetPhoneNumber()
		{
			return "+7"+Phone.Number;
		}

		public string GetCallerName()
		{
			return MangoManager.CallerName;
		}
		#endregion

		#region CallEvents
		public void FinishCallCommand()
		{
			MangoManager.HangUp();
			Close(false, CloseSource.Self);
		}

		public void ForwardCallCommand()
		{
			Action action =  () => { Close(false, CloseSource.Self); };
			IPage page = NavigationManager.OpenViewModelNamedArgs<SubscriberSelectionViewModel>
			(this, new Dictionary<string, object>()
				{ {"manager",MangoManager },{"dialogType", SubscriberSelectionViewModel.DialogType.AdditionalCall },
				{"exitAction", action}
			});
		}

		#endregion

	}
}
