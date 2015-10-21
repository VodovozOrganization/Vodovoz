using System;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Accounting;
using System.Collections.Generic;
using Vodovoz.Domain;
using QSBanks;
using NHibernate.Transform;
using QSProjectsLib;
using QSOrmProject;

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

			Counterparty counterpartyAlias;
			Account counterpartyAccountAlias;
			Bank counterpartyBankAlias;

			Organization organizationAlias;
			Account organizationAccountAlias;
			Bank organizationBankAlias;

//			Employee employeeAlias;
//			Account employeeAccountAlias;
//			Bank employeeBankAlias;

			List<AccountingVMNode> result = new List<AccountingVMNode> ();

			var income = UoW.Session.QueryOver<AccountIncome> (() => incomeAlias);

			var incomeList = income
				.JoinQueryOver (() => incomeAlias.Counterparty, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (() => incomeAlias.CounterpartyAccount, () => counterpartyAccountAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (() => counterpartyAccountAlias.InBank, () => counterpartyBankAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)

				.JoinQueryOver (() => incomeAlias.Organization, () => organizationAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (() => incomeAlias.OrganizationAccount, () => organizationAccountAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (() => organizationAccountAlias.InBank, () => organizationBankAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList (list => list
					.Select (() => incomeAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => incomeAlias.Date).WithAlias (() => resultAlias.Date)
					.Select (() => incomeAlias.Number).WithAlias (() => resultAlias.Number)
					.Select (() => incomeAlias.Description).WithAlias (() => resultAlias.Description)
					.Select (() => incomeAlias.Total).WithAlias (() => resultAlias.Income)
			           	
					.Select (() => counterpartyAlias.FullName).WithAlias (() => resultAlias.Payer)
					.Select (() => counterpartyAccountAlias.Number).WithAlias (() => resultAlias.PayerAccount)
					.Select (() => counterpartyBankAlias.Name).WithAlias (() => resultAlias.PayerBank)

					.Select (() => organizationAlias.FullName).WithAlias (() => resultAlias.Recipient)
					.Select (() => organizationAccountAlias.Number).WithAlias (() => resultAlias.RecipientAccount)
					.Select (() => organizationBankAlias.Name).WithAlias (() => resultAlias.RecipientBank)
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
				.SelectList (list => list
					.Select (() => expenseAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => expenseAlias.Date).WithAlias (() => resultAlias.Date)
					.Select (() => expenseAlias.Number).WithAlias (() => resultAlias.Number)
					.Select (() => expenseAlias.Description).WithAlias (() => resultAlias.Description)
					.Select (() => expenseAlias.Total).WithAlias (() => resultAlias.Expense)

					.Select (() => counterpartyAlias.FullName).WithAlias (() => resultAlias.Recipient)
					.Select (() => counterpartyAccountAlias.Number).WithAlias (() => resultAlias.PayerAccount)
					.Select (() => counterpartyBankAlias.Name).WithAlias (() => resultAlias.RecipientBank)

					.Select (() => organizationAlias.FullName).WithAlias (() => resultAlias.Payer)
					.Select (() => organizationAccountAlias.Number).WithAlias (() => resultAlias.PayerAccount)
					.Select (() => organizationBankAlias.Name).WithAlias (() => resultAlias.PayerBank)
			                  )
				.TransformUsing (Transformers.AliasToBean<AccountingVMNode> ())
				.List<AccountingVMNode> ();

			result.AddRange (expenseList);
		}

		#endregion

		Gtk.DataBindings.IMappingConfig treeViewConfig = Gtk.DataBindings.FluentMappingConfig<AccountingVMNode>.Create ()
			.AddColumn ("Номер").SetDataProperty (node => node.Number)
			.AddColumn ("Дата").SetDataProperty (node => node.Date.ToShortDateString ())
			.AddColumn ("Плательщик").SetDataProperty (node => node.PayerString)
			.AddColumn ("Приход").AddTextRenderer (node => CurrencyWorks.GetShortCurrencyString (node.Income))
			.AddColumn ("Расход").AddTextRenderer (node => CurrencyWorks.GetShortCurrencyString (node.Expense))
			.AddColumn ("Получатель").SetDataProperty (node => node.RecipientString)
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

		public string Number { get; set; }

		public DateTime Date { get; set; }

		public decimal Income { get; set; }

		public decimal Expense { get; set; }

		public string Payer { get; set; }

		public string PayerAccount { get; set; }

		public string PayerBank { get; set; }

		public string PayerString { get { return String.Format ("{0} (р/с {1} {2})", Payer, PayerAccount, PayerBank); } }

		public string Recipient { get; set; }

		public string RecipientAccount { get; set; }

		public string RecipientBank { get; set; }

		public string RecipientString { get { return String.Format ("{0} (р/с {1} {2})", Recipient, RecipientAccount, RecipientBank); } }

		public string Description { get; set; }
	}
}

