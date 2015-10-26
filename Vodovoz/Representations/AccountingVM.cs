using System;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Accounting;
using System.Collections.Generic;
using Vodovoz.Domain;
using QSBanks;
using NHibernate.Transform;
using QSProjectsLib;
using QSOrmProject;
using System.Linq;
using Gtk;
using Vodovoz.Domain.Cash;

namespace Vodovoz.ViewModel
{
	public class AccountingVM: RepresentationModelWithoutEntityBase<AccountingVMNode>
	{
		public AccountingVM () : this (UnitOfWorkFactory.CreateWithoutRoot ())
		{
		}

		public AccountingVM (IUnitOfWork uow) : base (typeof(AccountIncome), typeof(AccountExpense))
		{
			this.UoW = uow;
		}

		#region implemented abstract members of RepresentationModelBase

		public override void UpdateNodes ()
		{
			AccountingVMNode resultAlias;
			
			AccountIncome incomeAlias;
			AccountExpense expenseAlias;

			ExpenseCategory expenseCategoryAlias;
			IncomeCategory incomeCategoryAlias;

			Counterparty counterpartyAlias;
			Account counterpartyAccountAlias;
			Bank counterpartyBankAlias;

			Organization organizationAlias;
			Account organizationAccountAlias;
			Bank organizationBankAlias;

			Employee employeeAlias;
			Account employeeAccountAlias;
			Bank employeeBankAlias;

			List<AccountingVMNode> result = new List<AccountingVMNode> ();

			var income = UoW.Session.QueryOver<AccountIncome> (() => incomeAlias);

			var incomeList = income
				.JoinQueryOver (() => incomeAlias.Counterparty, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (() => incomeAlias.CounterpartyAccount, () => counterpartyAccountAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (() => counterpartyAccountAlias.InBank, () => counterpartyBankAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)

				.JoinQueryOver (() => incomeAlias.Organization, () => organizationAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (() => incomeAlias.OrganizationAccount, () => organizationAccountAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (() => organizationAccountAlias.InBank, () => organizationBankAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)

				.JoinQueryOver (() => incomeAlias.Category, () => incomeCategoryAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)

				.SelectList (list => list
					.Select (() => incomeAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => incomeAlias.Date).WithAlias (() => resultAlias.Date)
					.Select (() => incomeAlias.Number).WithAlias (() => resultAlias.Number)
					.Select (() => incomeAlias.Description).WithAlias (() => resultAlias.Description)
					.Select (() => incomeAlias.Total).WithAlias (() => resultAlias.Income)
			           	
					.Select (() => counterpartyAlias.FullName).WithAlias (() => resultAlias.Partner)
					.Select (() => counterpartyAccountAlias.Number).WithAlias (() => resultAlias.PartnerAccount)
					.Select (() => counterpartyBankAlias.Name).WithAlias (() => resultAlias.PartnerBank)

					.Select (() => organizationAccountAlias.Number).WithAlias (() => resultAlias.OrganizationAccount)
					.Select (() => organizationBankAlias.Name).WithAlias (() => resultAlias.OrganizationBank)
					.Select (() => incomeCategoryAlias.Name).WithAlias (() => resultAlias.Category)
			                 )
				.TransformUsing (Transformers.AliasToBean<AccountingVMNode> ())
				.List<AccountingVMNode> ();

			result.AddRange (incomeList);

			var expense = UoW.Session.QueryOver<AccountExpense> (() => expenseAlias);

			var expenseList = expense
				.JoinQueryOver (() => expenseAlias.Counterparty, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (() => expenseAlias.CounterpartyAccount, () => counterpartyAccountAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (() => counterpartyAccountAlias.InBank, () => counterpartyBankAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)

				.JoinQueryOver (() => expenseAlias.Organization, () => organizationAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (() => expenseAlias.OrganizationAccount, () => organizationAccountAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (() => organizationAccountAlias.InBank, () => organizationBankAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)

				.JoinQueryOver (() => expenseAlias.Employee, () => employeeAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (() => expenseAlias.EmployeeAccount, () => employeeAccountAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (() => employeeAccountAlias.InBank, () => employeeBankAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)

				.JoinQueryOver (() => expenseAlias.Category, () => expenseCategoryAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)

				.SelectList (list => list
					.Select (() => expenseAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => expenseAlias.Date).WithAlias (() => resultAlias.Date)
					.Select (() => expenseAlias.Number).WithAlias (() => resultAlias.Number)
					.Select (() => expenseAlias.Description).WithAlias (() => resultAlias.Description)
					.Select (() => expenseAlias.Total).WithAlias (() => resultAlias.Expense)

					.Select (() => counterpartyAlias.FullName).WithAlias (() => resultAlias.Partner)
					.Select (() => counterpartyAccountAlias.Number).WithAlias (() => resultAlias.PartnerAccount)
					.Select (() => counterpartyBankAlias.Name).WithAlias (() => resultAlias.PartnerBank)

					.Select (() => employeeAlias.Name).WithAlias (() => resultAlias.EmployeeName)
					.Select (() => employeeAlias.LastName).WithAlias (() => resultAlias.EmployeeLastName)
					.Select (() => employeeAlias.Patronymic).WithAlias (() => resultAlias.EmployeePatronymic)
					.Select (() => employeeAccountAlias.Number).WithAlias (() => resultAlias.EmployeeAccount)
					.Select (() => employeeBankAlias.Name).WithAlias (() => resultAlias.EmployeeBank)

					.Select (() => organizationAccountAlias.Number).WithAlias (() => resultAlias.OrganizationAccount)
					.Select (() => organizationBankAlias.Name).WithAlias (() => resultAlias.OrganizationBank)

					.Select (() => expenseCategoryAlias.Name).WithAlias (() => resultAlias.Category)
			                  )
				.TransformUsing (Transformers.AliasToBean<AccountingVMNode> ())
				.List<AccountingVMNode> ();

			result.AddRange (expenseList);

			SetItemsSource (result.OrderByDescending (d => d.Date).ToList ());
		}

		#endregion

		Gtk.DataBindings.IMappingConfig treeViewConfig = Gtk.DataBindings.FluentMappingConfig<AccountingVMNode>.Create ()
			.AddColumn ("Номер").SetDataProperty (node => node.Number.ToString ())
			.AddColumn ("Дата").SetDataProperty (node => node.Date.ToShortDateString ())
			.AddColumn ("Категория дохода/расхода").SetDataProperty (node => node.Category)
			.AddColumn ("Приход").AddTextRenderer (node => CurrencyWorks.GetShortCurrencyString (node.Income))
			.AddColumn ("Расход").AddTextRenderer (node => CurrencyWorks.GetShortCurrencyString (node.Expense))
			.AddColumn ("Контрагент/сотрудник").SetDataProperty (node => node.Name)
			.AddColumn ("Назначение").SetDataProperty (node => node.Description)
			.Finish ();

		public override Gtk.DataBindings.IMappingConfig TreeViewConfig {
			get { return treeViewConfig; }
		}

		#region implemented abstract members of RepresentationModelWithoutEntityBase

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			return true;
		}

		#endregion
	}

	public class AccountingVMNode
	{
		public int Id { get; set; }

		public int Number { get; set; }

		public DateTime Date { get; set; }

		public decimal Income { get; set; }

		public decimal Expense { get; set; }

		public string Category { get; set; }

		public string OrganizationAccount { get; set; }

		public string OrganizationBank { get; set; }

		public string Partner { get; set; }

		#region Удалить?

		public string PartnerAccount { get; set; }

		public string PartnerBank { get; set; }

		public string PartnerString { get { return String.Format ("{0} (р/с {1} {2})", Partner, PartnerAccount, PartnerBank); } }

		#endregion

		public string EmployeeName { get; set; }

		public string EmployeeLastName { get; set; }

		public string EmployeePatronymic { get; set; }

		public string Employee { get { return String.Format ("{0} {1} {2}", EmployeeLastName, EmployeeName, EmployeePatronymic); } }

		#region Удалить?

		public string EmployeeAccount { get; set; }

		public string EmployeeBank { get; set; }

		public string EmployeeString { get { return String.Format ("{0} (р/с {1} {2})", Employee, EmployeeAccount, EmployeeBank); } }

		#endregion

		public string Name { get { return String.IsNullOrWhiteSpace (Partner) ? Employee : Partner; } }

		public string Description { get; set; }
	}
}

