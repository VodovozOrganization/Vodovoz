using System;
using System.Collections.Generic;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	public partial class CashExpenseDlg : OrmGtkDialogBase<Expense>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		private decimal currentEmployeeWage = default(decimal);

		public CashExpenseDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Expense>();
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

		public CashExpenseDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Expense> (id);
			ConfigureDlg ();
		}

		public CashExpenseDlg (Expense sub) : this (sub.Id) {}

		void ConfigureDlg()
		{
			enumcomboOperation.ItemsEnum = typeof(ExpenseType);
			enumcomboOperation.Binding.AddBinding (Entity, s => s.TypeOperation, w => w.SelectedItem).InitializeFromSource ();

			yentryCasher.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeOrderedQuery ();
			yentryCasher.SetObjectDisplayFunc<Employee> (e => e.ShortName);
			yentryCasher.Binding.AddBinding (Entity, s => s.Casher, w => w.Subject).InitializeFromSource ();

			yentryEmploeey.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeOrderedQuery ();
			yentryEmploeey.SetObjectDisplayFunc<Employee> (e => e.ShortName);
			yentryEmploeey.Binding.AddBinding (Entity, s => s.Employee, w => w.Subject).InitializeFromSource ();
			yentryEmploeey.ChangedByUser += (sender, e) => UpdateEmployeeBalaceInfo();

			ydateDocument.Binding.AddBinding (Entity, s => s.Date, w => w.Date).InitializeFromSource ();

			OrmMain.GetObjectDescription<ExpenseCategory> ().ObjectUpdated += OnExpenseCategoryUpdated;
			OnExpenseCategoryUpdated (null, null);
			comboExpense.Binding.AddBinding (Entity, s => s.ExpenseCategory, w => w.SelectedItem).InitializeFromSource ();

			yspinMoney.Binding.AddBinding (Entity, s => s.Money, w => w.ValueAsDecimal).InitializeFromSource ();

			ytextviewDescription.Binding.AddBinding (Entity, s => s.Description, w => w.Buffer.Text).InitializeFromSource ();

			ExpenseType type = (ExpenseType)enumcomboOperation.SelectedItem;
			ylabelEmployeeWageBalance.Visible = type == ExpenseType.EmployeeAdvance
											 || type == ExpenseType.Salary;
			UpdateEmployeeBalaceInfo();
		}

		void OnExpenseCategoryUpdated (object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedEventArgs e)
		{
			comboExpense.ItemsList = Repository.Cash.CategoryRepository.ExpenseCategories (UoW);
		}

		public override bool Save ()
		{
			var valid = new QSValidator<Expense> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			Entity.UpdateWagesOperations(UoW);

			logger.Info ("Сохраняем расходный ордер...");
			UoWGeneric.Save();
			logger.Info ("Ok");
			return true;

		}

		protected void OnEnumcomboOperationEnumItemSelected (object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			UpdateEmployeeBalaceInfo();

			switch((ExpenseType)e.SelectedItem)
			{
				case ExpenseType.Advance: 
					labelEmploeey.LabelProp = "Подотчетное лицо:";
					ylabelEmployeeWageBalance.Visible = false;
					break;
				case ExpenseType.Expense : 
					labelEmploeey.LabelProp = "Сотрудник:";
					ylabelEmployeeWageBalance.Visible = false;
					break;
				case ExpenseType.EmployeeAdvance:
					labelEmploeey.LabelProp = "Сотрудник:";
					ylabelEmployeeWageBalance.Visible = true;
					break;
				case ExpenseType.Salary:
					labelEmploeey.LabelProp = "Сотрудник:";
					ylabelEmployeeWageBalance.Visible = true;
					break;
			}
		}

		private void UpdateEmployeeBalaceInfo()
		{
			currentEmployeeWage = 0;
			string labelTemplate = "Текущий баланс сотрудника: {0}";
			Employee employee = yentryEmploeey.Subject as Employee;

			if (employee != null)
			{
				currentEmployeeWage =
					Repository.Operations.WagesMovementRepository.GetCurrentEmployeeWageBalance(UoW, employee.Id);
			}

			ylabelEmployeeWageBalance.LabelProp = string.Format(labelTemplate, currentEmployeeWage);
		}

		protected void OnButtonPrintClicked (object sender, EventArgs e)
		{
			if (UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint (typeof(Expense), "квитанции"))
				Save ();

			var reportInfo = new QSReport.ReportInfo {
				Title = String.Format ("Квитанция №{0} от {1:d}", Entity.Id, Entity.Date),
				Identifier = "Cash.Expense",
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id }
				}
			};
				
			var report = new QSReport.ReportViewDlg (reportInfo);
			TabParent.AddTab (report, this, false);
		}
	}
}

