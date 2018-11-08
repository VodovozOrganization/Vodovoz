using System;
using QS.DomainModel.UoW;
using QSOrmProject;
using QS.Tdi;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class AccountableDebts : QS.Dialog.Gtk.TdiTabBase
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
			buttonSlips.Sensitive = buttonAdvanceReport.Sensitive = buttonUnclosed.Sensitive = representationtreeviewDebts.Selection.CountSelectedRows () > 0;
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
			var accountable = UoW.GetById<Employee> (representationtreeviewDebts.GetSelectedId ());
			var category = accountabledebtsfilter1.RestrictExpenseCategory;
			decimal money = representationtreeviewDebts.GetSelectedObject<AccountableDebtsVMNode> ().Debt;

			var dlg = new AdvanceReportDlg (accountable, category, money);
			OpenNewTab (dlg);
		}

		protected void OnButtonSlipsClicked (object sender, EventArgs e)
		{
			var accountable = UoW.GetById<Employee> (representationtreeviewDebts.GetSelectedId ());
			var category = accountabledebtsfilter1.RestrictExpenseCategory;

			var dlg = new AccountableSlipsView (accountable, category);
			OpenNewTab (dlg);
		}

		protected void OnRepresentationtreeviewDebtsRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonSlips.Click ();
		}

		protected void OnButtonUnclosedClicked (object sender, EventArgs e)
		{
			var accountable = UoW.GetById<Employee> (representationtreeviewDebts.GetSelectedId ());
			var category = accountabledebtsfilter1.RestrictExpenseCategory;

			var dlg = new UnclosedAdvancesView (accountable, category);
			OpenNewTab (dlg);

		}
	}
}

