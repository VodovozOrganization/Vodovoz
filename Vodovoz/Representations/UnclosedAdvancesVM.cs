using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSProjectsLib;
using Vodovoz.Domain;
using Vodovoz.Domain.Cash;
using NHibernate;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;

namespace Vodovoz.ViewModel
{
	public class UnclosedAdvancesVM : RepresentationModelWithoutEntityBase<UnclosedAdvancesVMNode>
	{
		public UnclosedAdvancesFilter Filter {
			get {
				return RepresentationFilter as UnclosedAdvancesFilter;
			}
			set { RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			UnclosedAdvancesVMNode resultAlias = null;

			Employee employeeAlias = null;
			Employee casherAlias = null;

			Expense expenseAlias = null;
			ExpenseCategory expenseCategoryAlias = null;

			var expense = UoW.Session.QueryOver<Expense> (() => expenseAlias)
				.Where (e => e.AdvanceClosed == false && e.TypeOperation == ExpenseType.Advance);

			if (Filter.RestrictExpenseCategory != null)
				expense.Where (i => i.ExpenseCategory == Filter.RestrictExpenseCategory);
			if (Filter.RestrictAccountable != null)
				expense.Where (o => o.Employee == Filter.RestrictAccountable);

			var expenseList = expense
				.JoinQueryOver (() => expenseAlias.Employee, () => employeeAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (() => expenseAlias.Casher, () => casherAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (() => expenseAlias.ExpenseCategory, () => expenseCategoryAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList (list => list
					.Select (() => expenseAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => expenseAlias.Date).WithAlias (() => resultAlias.Date)
					.Select (() => expenseAlias.Money).WithAlias (() => resultAlias.Money)
					.Select (() => expenseAlias.Description).WithAlias (() => resultAlias.Description)
					.Select (() => employeeAlias.Name).WithAlias (() => resultAlias.EmployeeName)
					.Select (() => employeeAlias.LastName).WithAlias (() => resultAlias.EmployeeSurname)
					.Select (() => employeeAlias.Patronymic).WithAlias (() => resultAlias.EmployeePatronymic)
					.Select (() => casherAlias.Name).WithAlias (() => resultAlias.CasherName)
					.Select (() => casherAlias.LastName).WithAlias (() => resultAlias.CasherSurname)
					.Select (() => casherAlias.Patronymic).WithAlias (() => resultAlias.CasherPatronymic)
					.Select (() => expenseCategoryAlias.Name).WithAlias (() => resultAlias.Category)
				)
				.TransformUsing (Transformers.AliasToBean<UnclosedAdvancesVMNode> ())
				.List<UnclosedAdvancesVMNode> ();

			SetItemsSource (expenseList.OrderByDescending (d => d.Date).ToList ());
		}

		IColumnsConfig treeViewConfig = ColumnsConfigFactory.Create<UnclosedAdvancesVMNode>()
			//.AddColumn ("Номер").SetDataProperty (node => node.Id.ToString())
			.AddColumn ("Дата").SetDataProperty (node => node.DateString)
			.AddColumn ("Кассир").SetDataProperty (node => node.CasherString)
			.AddColumn ("Статья").SetDataProperty (node => node.Category)
			.AddColumn ("Сумма").AddTextRenderer (node => CurrencyWorks.GetShortCurrencyString (node.Money))
			.AddColumn ("Сотрудник").SetDataProperty (node => node.EmployeeString)
			.AddColumn ("Основание").SetDataProperty (node => node.Description)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return treeViewConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			return true;
		}

		#endregion

		public UnclosedAdvancesVM (UnclosedAdvancesFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public UnclosedAdvancesVM () : this(UnitOfWorkFactory.CreateWithoutRoot ())
		{
			CreateRepresentationFilter = () => new UnclosedAdvancesFilter(UoW);
		}

		public UnclosedAdvancesVM (IUnitOfWork uow) : base (
			typeof(Expense))
		{
			this.UoW = uow;
		}
	}

	public class UnclosedAdvancesVMNode
	{

		public int Id { get; set; }

		public DateTime Date { get; set; }

		public string DateString { get { return  Date.ToString (); } }

		public string EmployeeSurname { get; set; }
		public string EmployeeName { get; set; }
		public string EmployeePatronymic { get; set; }

		public string EmployeeString {
			get {
				return StringWorks.PersonNameWithInitials (EmployeeSurname, EmployeeName, EmployeePatronymic);
			}
		}

		public string CasherSurname { get; set; }
		public string CasherName { get; set; }
		public string CasherPatronymic { get; set; }

		public string CasherString {
			get {
				return StringWorks.PersonNameWithInitials (CasherSurname, CasherName, CasherPatronymic);
			}
		}

		public string Category { get; set; }

		public string Description { get; set; }

		public decimal Money { get; set; } 
	}
}

