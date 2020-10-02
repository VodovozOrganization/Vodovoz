using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Utilities.Numeric;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure.Mango;

namespace Vodovoz.ViewModels.Mango
{
	public class IncomingCallViewModel : WindowDialogViewModelBase
	{
		public readonly MangoManager MangoManager;
		public readonly IUnitOfWork UoW;
		public readonly bool IsTransfer = false;
		private string onLine = null;
		public string OnLine { get => onLine; private set => onLine = value; }


		public IncomingCallViewModel(
			IUnitOfWorkFactory UoWFactory, 
			INavigationManager navigation,
			ITdiCompatibilityNavigation tdinavigation,
			MangoManager manager) : base(navigation)
		{
			this.MangoManager = manager ?? throw new ArgumentNullException(nameof(manager));
			UoW = UoWFactory.CreateWithoutRoot() ?? throw new ArgumentNullException(nameof(UoWFactory));
			if(manager.IsTransfer && manager.PrimaryCaller != null) {
				IsTransfer = manager.IsTransfer;
				if(manager.PrimaryCaller != null) {
					if(manager.Employee != 0) {
						Employee employee = UoW.GetById<Employee>(manager.Employee);
						onLine = employee.Name;
					}
					else {
						if(MangoManager.PrimaryCaller.Number.Length == 11) {
							var formatter = new PhoneFormatter(PhoneFormat.BracketWithWhitespaceLastTen);
							string loc = "+7" + formatter.FormatString(manager.PrimaryCaller.Number);
							onLine = loc;
						} else onLine = manager.PrimaryCaller.Number;
					}
				}
			}
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
			Close(false, CloseSource.Self);
		}

		#endregion
	}
}
