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
				accountabledebtsfilter1.UoW = value;
				var vm = new ViewModel.AccountableDebtsVM (accountabledebtsfilter1);
				representationtreeviewDebts.RepresentationModel = vm;
				representationtreeviewDebts.RepresentationModel.UpdateNodes ();
			}
		}

		public AccountableDebts ()
		{
			this.Build ();
			this.TabName = "Долги сотрудников";
			representationtreeviewDebts.Selection.Changed += RepresentationtreeviewDebts_Selection_Changed;
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
		}

		void RepresentationtreeviewDebts_Selection_Changed (object sender, EventArgs e)
		{
			buttonSlips.Sensitive = buttonAdvanceReport.Sensitive = representationtreeviewDebts.Selection.CountSelectedRows () > 0;
		}

		protected void OnButtonSearchClearClicked (object sender, EventArgs e)
		{
			entrySearch.Text = String.Empty;
		}

		protected void OnEntrySearchChanged (object sender, EventArgs e)
		{
			representationtreeviewDebts.RepresentationModel.SearchString = entrySearch.Text;
		}

		protected void OnButtonAdvanceReportClicked (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}
	}
}

