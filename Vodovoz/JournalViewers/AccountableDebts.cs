using System;
using QSOrmProject;
using QSTDI;

namespace Vodovoz
{
	public partial class AccountableDebts : TdiTabBase
	{
		private IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				if (uow == value)
					return;
				uow = value;
				var vm = new ViewModel.AccountableDebtsVM (value);
				representationtreeviewDebts.RepresentationModel = vm;
				representationtreeviewDebts.RepresentationModel.UpdateNodes ();
			}
		}


		public AccountableDebts ()
		{
			this.Build ();
			this.TabName = "Долги сотрудников";
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
		}
	}
}

