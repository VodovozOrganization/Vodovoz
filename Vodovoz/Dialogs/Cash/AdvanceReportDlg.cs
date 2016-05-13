using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	public partial class AdvanceReportDlg : OrmGtkDialogBase<AdvanceReport>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		decimal debt = 0;
		decimal Balance = 0;
		decimal closingSum = 0;

		List<RecivedAdvance> advanceList;

		protected decimal Debt {
			get {
				return debt;
			}
			set {
				debt = value;
				labelCurrentDebt.LabelProp = String.Format ("{0:C}", debt);
			}
		}

		protected decimal ClosingSum {
			get {
				return closingSum;
			}
			set {
				closingSum = value;
				labelClosingSum.LabelProp = String.Format ("{0:C}", closingSum);
				CalculateBalance ();
			}
		}

		public AdvanceReportDlg (Expense advance) : this(advance.Employee, advance.ExpenseCategory, advance.UnclosedMoney)
		{
			advanceList.Find(x => x.Advance.Id == advance.Id).Selected = true;
		}

		public AdvanceReportDlg (Employee accountable, ExpenseCategory expenseCategory, decimal money) : this()
		{
			Entity.Accountable = accountable;
			Entity.ExpenseCategory = expenseCategory;
			Entity.Money = money;
		}

		public AdvanceReportDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<AdvanceReport>();
			Entity.Casher = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if(Entity.Casher == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать кассовые документы, так как некого указывать в качестве кассира.");
				FailInitialize = true;
				return;
			}
			Entity.Date = DateTime.Now;
			ConfigureDlg ();
			FillDebt ();
		}

		public AdvanceReportDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<AdvanceReport> (id);
			//Отключаем отображение ненужных элементов.
			labelDebtTitle.Visible = labelTableTitle.Visible = hboxDebt.Visible = GtkScrolledWindow1.Visible = labelCreating.Visible = false;

			comboExpense.Sensitive = yspinMoney.Sensitive = yentryEmploeey.Sensitive = false;

			ConfigureDlg ();
		}

		public AdvanceReportDlg (AdvanceReport sub) : this (sub.Id) {}

		void ConfigureDlg()
		{
			yentryCasher.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeOrderedQuery ();
			yentryCasher.SetObjectDisplayFunc<Employee> (e => e.ShortName);
			yentryCasher.Binding.AddBinding (Entity, s => s.Casher, w => w.Subject).InitializeFromSource ();

			yentryEmploeey.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeOrderedQuery ();
			yentryEmploeey.SetObjectDisplayFunc<Employee> (e => e.ShortName);
			yentryEmploeey.Binding.AddBinding (Entity, s => s.Accountable, w => w.Subject).InitializeFromSource ();

			ydateDocument.Binding.AddBinding (Entity, s => s.Date, w => w.Date).InitializeFromSource ();

			OrmMain.GetObjectDescription<ExpenseCategory> ().ObjectUpdated += OnExpenseCategoryUpdated;
			OnExpenseCategoryUpdated (null, null);
			comboExpense.Binding.AddBinding (Entity, s => s.ExpenseCategory, w => w.SelectedItem).InitializeFromSource ();

			yspinMoney.Binding.AddBinding (Entity, s => s.Money, w => w.ValueAsDecimal).InitializeFromSource ();

			ytextviewDescription.Binding.AddBinding (Entity, s => s.Description, w => w.Buffer.Text).InitializeFromSource ();

			ytreeviewDebts.ColumnsConfig = ColumnsConfigFactory.Create<RecivedAdvance> ()
				.AddColumn ("Закрыть").AddToggleRenderer (a => a.Selected).Editing ()
				.AddColumn ("Дата").AddTextRenderer (a => a.Advance.Date.ToString ())
				.AddColumn ("Получено").AddTextRenderer (a => a.Advance.Money.ToString ("C"))
				.AddColumn ("Непогашено").AddTextRenderer (a => a.Advance.UnclosedMoney.ToString ("C"))
				.AddColumn ("Статья").AddTextRenderer (a => a.Advance.ExpenseCategory.Name)
				.AddColumn ("Основание").AddTextRenderer (a => a.Advance.Description)
				.Finish ();
		}

		void OnExpenseCategoryUpdated (object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedEventArgs e)
		{
			comboExpense.ItemsList = Repository.Cash.CategoryRepository.ExpenseCategories (UoW);
		}

		public override bool Save ()
		{
			var valid = new QSValidator<AdvanceReport> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем авансовый отчет...");
			Income newIncome;
			Expense newExpense;
			bool needClosing = UoWGeneric.IsNew;
			UoWGeneric.Save(); // Сохраняем сначала отчет, так как нужно получить Id.
			if(needClosing)
			{
				var closing = Entity.CloseAdvances (out newExpense, out newIncome, 
					advanceList.Where (a => a.Selected).Select (a => a.Advance).ToList ());

				if(newExpense != null)
					UoWGeneric.Save (newExpense);
				if(newIncome != null)
					UoWGeneric.Save (newIncome);

				advanceList.Where (a => a.Selected).Select (a => a.Advance).ToList ().ForEach (a => UoWGeneric.Save (a));
				closing.ForEach (c => UoWGeneric.Save(c));

				UoWGeneric.Save ();

				if(newIncome != null)
				{
					MessageDialogWorks.RunInfoDialog (String.Format ("Дополнительно создан приходный ордер №{0}, на сумму {1:C}.\nНе забудьте получить сдачу от подотчетного лица!",
						newIncome.Id, newIncome.Money));
				}
				if(newExpense != null)
				{
					MessageDialogWorks.RunInfoDialog (String.Format ("Дополнительно создан расходный ордер №{0}, на сумму {1:C}.\nНе забудьте доплатить подотчетному лицу!",
						newExpense.Id, newExpense.Money));
				}
			}
			logger.Info ("Ok");
			return true;
		}

		protected void CalculateBalance()
		{
			if (!UoW.IsNew)
				return;

			Balance = ClosingSum - Entity.Money;

			labelChangeSum.Visible = labelChangeType.Visible = true;

			if(ClosingSum == 0)
			{
				labelChangeSum.Visible = labelChangeType.Visible = false;
				labelCreating.Markup = String.Format("<span foreground=\"Cadet Blue\">Не выбранных авансов.</span>");
			}
			else if(Balance == 0)
			{
				labelChangeSum.Visible = labelChangeType.Visible = false;
				labelCreating.Markup = String.Format("<span foreground=\"green\">Аванс будет закрыть полностью.</span>");
			}
			else if(Balance < 0)
			{
				labelChangeType.LabelProp = "Доплата:";
				labelChangeSum.LabelProp = string.Format("<span foreground=\"red\">{0:C}</span>", Math.Abs(Balance));
				labelCreating.Markup = String.Format("<span foreground=\"blue\">Будет создан расходный ордер на сумму {0:C}, в качестве доплаты.</span>", Math.Abs(Balance));
			}
			else
			{
				labelChangeType.LabelProp = "Остаток:";
				labelChangeSum.LabelProp = string.Format("{0:C}", Balance);
				labelCreating.Markup = String.Format ("<span foreground=\"blue\">Будет создан приходный ордер на сумму {0:C}, в качестве сдачи от подотчетного лица.</span>", Math.Abs(Balance));
			}
		}

		protected void OnYspinMoneyValueChanged (object sender, EventArgs e)
		{
			CalculateBalance ();
		}

		void FillDebt()
		{
			if(!UoW.IsNew)
				return;

			if(Entity.Accountable == null)
			{
				Debt = 0;
				ytreeviewDebts.Model = null;
				return;
			}

			logger.Info("Получаем долг {0}...", Entity.Accountable.ShortName);
			//Debt = Repository.Cash.AccountableDebtsRepository.EmloyeeDebt (UoW, Entity.Accountable);

			var advaces = Repository.Cash.AccountableDebtsRepository.UnclosedAdvance (UoW, Entity.Accountable, Entity.ExpenseCategory);

			Debt = advaces.Sum (a => a.UnclosedMoney);

			advanceList = new List<RecivedAdvance> ();

			advaces.ToList ().ForEach (adv => advanceList.Add (new RecivedAdvance(adv)));
			advanceList.ForEach (i => i.SelectChanged += I_SelectChanged);
			ytreeviewDebts.ItemsDataSource = advanceList;

			CalculateBalance ();
			logger.Info("Ok");
		}

		void I_SelectChanged (object sender, EventArgs e)
		{
			ClosingSum = advanceList.Where (a => a.Selected).Sum (a => a.Advance.UnclosedMoney);
		}

		protected void OnYentryEmploeeyChanged (object sender, EventArgs e)
		{
			FillDebt ();
		}

		protected void OnComboExpenseChanged (object sender, EventArgs e)
		{
			FillDebt ();
		}
			
		class RecivedAdvance
		{
			private bool selected;

			public bool Selected {
				get { return selected;}
				set{ selected = value;
					if (SelectChanged != null)
						SelectChanged (this, EventArgs.Empty);
				}
			}

			public event EventHandler SelectChanged;

			public Expense Advance { get; set;}

			public RecivedAdvance(Expense exp)
			{
				Advance = exp;
				Selected = false;
			}
		}
	}
}

