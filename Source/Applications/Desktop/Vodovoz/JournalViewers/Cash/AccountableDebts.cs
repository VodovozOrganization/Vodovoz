using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels.Cash;

namespace Vodovoz
{
	public partial class AccountableDebts : QS.Dialog.Gtk.TdiTabBase
	{
		private IUnitOfWork _uow;

		public IUnitOfWork UoW
		{
			get
			{
				return _uow;
			}
			set
			{
				if(_uow == value)
				{
					return;
				}

				_uow = value;
				accountabledebtsfilter1.UoW = value;
				var vm = new ViewModel.AccountableDebtsVM(accountabledebtsfilter1);
				representationtreeviewDebts.RepresentationModel = vm;
				representationtreeviewDebts.RepresentationModel.UpdateNodes();
				accountabledebtsfilter1.JournalTab = this;
			}
		}

		public INavigationManager NavigationManager { get; set; }

		public AccountableDebts(INavigationManager navigationManager)
		{
			Build();
			TabName = "Долги сотрудников";
			representationtreeviewDebts.Selection.Changed += RepresentationtreeviewDebts_Selection_Changed;
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
		}

		void RepresentationtreeviewDebts_Selection_Changed(object sender, EventArgs e)
		{
			buttonSlips.Sensitive = buttonAdvanceReport.Sensitive = buttonUnclosed.Sensitive = representationtreeviewDebts.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonSearchClearClicked(object sender, EventArgs e)
		{
			entrySearch.Text = string.Empty;
		}

		protected void OnEntrySearchChanged(object sender, EventArgs e)
		{
			representationtreeviewDebts.RepresentationModel.SearchString = entrySearch.Text;
		}

		protected void OnButtonAdvanceReportClicked(object sender, EventArgs e)
		{
			var accountable = UoW.GetById<Employee>(representationtreeviewDebts.GetSelectedId());
			var expenseCategoryId = accountabledebtsfilter1.FinancialExpenseCategory?.Id;
			decimal money = representationtreeviewDebts.GetSelectedObject<AccountableDebtsVMNode>().Debt;

			var page = NavigationManager.OpenViewModel<AdvanceReportViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForCreate());

			page.ViewModel.Entity.Accountable = accountable;
			page.ViewModel.Entity.ExpenseCategoryId = expenseCategoryId;
			page.ViewModel.Money = money;
		}

		protected void OnButtonSlipsClicked(object sender, EventArgs e)
		{
			var accountable = UoW.GetById<Employee>(representationtreeviewDebts.GetSelectedId());
			var category = accountabledebtsfilter1.FinancialExpenseCategory;

			var dlg = new AccountableSlipsView(accountable, category);
			OpenNewTab(dlg);
		}

		protected void OnRepresentationtreeviewDebtsRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			buttonSlips.Click();
		}

		protected void OnButtonUnclosedClicked(object sender, EventArgs e)
		{
			var accountable = UoW.GetById<Employee>(representationtreeviewDebts.GetSelectedId());
			var category = accountabledebtsfilter1.FinancialExpenseCategory;

			var dlg = new UnclosedAdvancesView(accountable, category, NavigationManager);
			OpenNewTab(dlg);
		}

		public override void Destroy()
		{
			UoW?.Dispose();
			base.Destroy();
		}
	}
}

