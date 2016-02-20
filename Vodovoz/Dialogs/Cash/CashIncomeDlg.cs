using System;
using System.Collections.Generic;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Cash;
using Gamma.GtkWidgets;
using System.Linq;

namespace Vodovoz
{
	public partial class CashIncomeDlg : OrmGtkDialogBase<Income>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		List<Selectable<Expense>> selectableAdvances;

		public CashIncomeDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Income>();
			Entity.Casher = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if(Entity.Casher == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать кассовые документы, так как некого указывать в качестве кассира.");
				FailInitialize = true;
				return;
			}
			Entity.Date = DateTime.Now;
			ConfigureDlg ();
		}

		public CashIncomeDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Income> (id);
			ConfigureDlg ();
		}

		public CashIncomeDlg (Expense advance) : this () 
		{
			Entity.TypeOperation = IncomeType.Return;
			Entity.ExpenseCategory = advance.ExpenseCategory;
			Entity.Employee = advance.Employee;
			selectableAdvances.Find(x => x.Value.Id == advance.Id).Selected = true;
		}

		public CashIncomeDlg (Income sub) : this (sub.Id) {}

		void ConfigureDlg()
		{
			enumcomboOperation.ItemsEnum = typeof(IncomeType);
			enumcomboOperation.Binding.AddBinding (Entity, s => s.TypeOperation, w => w.SelectedItem).InitializeFromSource ();

			yentryCasher.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeOrderedQuery ();
			yentryCasher.SetObjectDisplayFunc<Employee> (e => e.ShortName);
			yentryCasher.Binding.AddBinding (Entity, s => s.Casher, w => w.Subject).InitializeFromSource ();

			yentryEmploeey.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeOrderedQuery ();
			yentryEmploeey.SetObjectDisplayFunc<Employee> (e => e.ShortName);
			yentryEmploeey.Binding.AddBinding (Entity, s => s.Employee, w => w.Subject).InitializeFromSource ();

			yentryClient.ItemsQuery = Repository.CounterpartyRepository.ActiveClientsQuery ();
			yentryClient.Binding.AddBinding (Entity, s => s.Customer, w => w.Subject).InitializeFromSource ();

			ydateDocument.Binding.AddBinding (Entity, s => s.Date, w => w.Date).InitializeFromSource ();

			OrmMain.GetObjectDescription<ExpenseCategory> ().ObjectUpdated += OnExpenseCategoryUpdated;
			OnExpenseCategoryUpdated (null, null);
			comboExpense.Binding.AddBinding (Entity, s => s.ExpenseCategory, w => w.SelectedItem).InitializeFromSource ();

			OrmMain.GetObjectDescription<IncomeCategory> ().ObjectUpdated += OnIncomeCategoryUpdated;
			OnIncomeCategoryUpdated (null, null);
			comboCategory.Binding.AddBinding (Entity, s => s.IncomeCategory, w => w.SelectedItem).InitializeFromSource ();

			checkNoClose.Binding.AddBinding(Entity, e => e.NoFullCloseMode, w => w.Active);

			yspinMoney.Binding.AddBinding (Entity, s => s.Money, w => w.ValueAsDecimal).InitializeFromSource ();

			ytextviewDescription.Binding.AddBinding (Entity, s => s.Description, w => w.Buffer.Text).InitializeFromSource ();

			ytreeviewDebts.ColumnsConfig = ColumnsConfigFactory.Create<Selectable<Expense>> ()
				.AddColumn ("Закрыть").AddToggleRenderer (a => a.Selected).Editing ()
				.AddColumn ("Дата").AddTextRenderer (a => a.Value.Date.ToString ())
				.AddColumn ("Получено").AddTextRenderer (a => a.Value.Money.ToString ("C"))
				.AddColumn ("Непогашено").AddTextRenderer (a => a.Value.UnclosedMoney.ToString ("C"))
				.AddColumn ("Статья").AddTextRenderer (a => a.Value.ExpenseCategory.Name)
				.AddColumn ("Основание").AddTextRenderer (a => a.Value.Description)
				.Finish ();
		}

		void OnIncomeCategoryUpdated (object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedEventArgs e)
		{
			comboCategory.ItemsList = Repository.Cash.CategoryRepository.IncomeCategories (UoW);
		}

		void OnExpenseCategoryUpdated (object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedEventArgs e)
		{
			comboExpense.ItemsList = Repository.Cash.CategoryRepository.ExpenseCategories (UoW);
		}

		public override bool Save ()
		{
			if (Entity.TypeOperation == IncomeType.Return && UoW.IsNew)
				Entity.PrepareCloseAdvance(selectableAdvances.Where(x => x.Selected).Select(x => x.Value).ToList());

			var valid = new QSValidator<Income> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем Приходный ордер..."); 
			if (Entity.TypeOperation == IncomeType.Return && UoW.IsNew) {
				logger.Info ("Закрываем авансы...");
				Entity.CloseAdvances(UoW);
			}
			UoWGeneric.Save();
			logger.Info ("Ok");
			return true;
		}
			
		protected void OnButtonPrintClicked (object sender, EventArgs e)
		{
			if (UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint (typeof(Expense), "квитанции"))
				Save ();

			var reportInfo = new QSReport.ReportInfo {
				Title = String.Format ("Квитанция №{0} от {1:d}", Entity.Id, Entity.Date),
				Identifier = "Cash.ReturnTicket",
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id }
				}
			};

			var report = new QSReport.ReportViewDlg (reportInfo);
			TabParent.AddTab (report, this, false);
		}

		protected void OnEnumcomboOperationEnumItemSelected (object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			buttonPrint.Sensitive = Entity.TypeOperation == IncomeType.Return;
			labelExpenseTitle.Visible = comboExpense.Visible = Entity.TypeOperation == IncomeType.Return;
			labelIncomeTitle.Visible = comboCategory.Visible = Entity.TypeOperation != IncomeType.Return;

			labelClientTitle.Visible = yentryClient.Visible = Entity.TypeOperation == IncomeType.Payment;

			vboxDebts.Visible = checkNoClose.Visible = Entity.TypeOperation == IncomeType.Return && UoW.IsNew;
			yspinMoney.Sensitive = Entity.TypeOperation != IncomeType.Return;
			yspinMoney.ValueAsDecimal = 0;

			FillDebts ();
		}

		protected void OnYentryEmploeeyChanged (object sender, EventArgs e)
		{			
			FillDebts ();
		}

		protected void OnComboExpenseItemSelected (object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			FillDebts ();
		}

		protected void FillDebts(){
			if (Entity.TypeOperation == IncomeType.Return && Entity.Employee != null) {
				var advances = Repository.Cash.AccountableDebtsRepository
					.UnclosedAdvance (UoW, Entity.Employee, Entity.ExpenseCategory);
				selectableAdvances = advances.Select (advance => new Selectable<Expense> (advance))
				.ToList ();
				selectableAdvances.ForEach (advance => advance.SelectChanged += OnAdvanceSelectionChanged);
				ytreeviewDebts.ItemsDataSource = selectableAdvances;
			}
		}

		protected void OnAdvanceSelectionChanged(object sender, EventArgs args){
			if(checkNoClose.Active && (sender as Selectable<Expense>).Selected)
			{
				selectableAdvances.Where(x => x != sender).ToList().ForEach(x => x.SilentUnselect());
			}

			Entity.Money = selectableAdvances.
				Where(expense=>expense.Selected)
				.Sum (selectedExpense => selectedExpense.Value.UnclosedMoney);
		}
			
		protected void OnCheckNoCloseToggled(object sender, EventArgs e)
		{
			if (selectableAdvances == null)
				return;
			if(checkNoClose.Active && selectableAdvances.Count(x => x.Selected) > 1)
			{
				MessageDialogWorks.RunWarningDialog("Частично вернуть можно только один аванс.");
				checkNoClose.Active = false;
				return;
			}
			yspinMoney.Sensitive = checkNoClose.Active;
			if(!checkNoClose.Active)
			{
				yspinMoney.ValueAsDecimal = selectableAdvances.Where(x => x.Selected).Sum(x => x.Value.UnclosedMoney);
			}
		}
	}

	public class Selectable<T> {

		private bool selected;

		public bool Selected {
			get { return selected;}
			set{ selected = value;
				if (SelectChanged != null)
					SelectChanged (this, EventArgs.Empty);
			}
		}

		public event EventHandler SelectChanged;

		public void SilentUnselect()
		{
			selected = false;
		}

		public T Value { get; set;}

		public Selectable(T obj)
		{
			Value = obj;
			Selected = false;
		}
	}
}

