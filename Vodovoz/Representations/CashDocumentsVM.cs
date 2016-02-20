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
using NHibernate.Criterion;
using NHibernate;
using Gamma.ColumnConfig;

namespace Vodovoz.ViewModel
{
	public class CashDocumentsVM : RepresentationModelWithoutEntityBase<CashDocumentsVMNode>
	{
		public CashDocumentsFilter Filter {
			get {
				return RepresentationFilter as CashDocumentsFilter;
			}
			set { RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			CashDocumentsVMNode resultAlias = null;

			Income incomeAlias = null;
			Employee employeeAlias = null;
			Employee casherAlias = null;
			IncomeCategory incomeCategoryAlias = null;

			Expense expenseAlias = null;
			ExpenseCategory expenseCategoryAlias = null;

			AdvanceReport advanceReportAlias = null;

			List<CashDocumentsVMNode> result = new List<CashDocumentsVMNode> ();

			if (Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == CashDocumentType.Income) {

				var income = UoW.Session.QueryOver<Income> (() => incomeAlias);

				if (Filter.RestrictExpenseCategory != null)
					income.Where (i => i.ExpenseCategory == Filter.RestrictExpenseCategory);
				if (Filter.RestrictIncomeCategory != null)
					income.Where (i => i.IncomeCategory == Filter.RestrictIncomeCategory);
				if(Filter.RestrictStartDate.HasValue)
					income.Where (o => o.Date >= Filter.RestrictStartDate.Value);
				if(Filter.RestrictEndDate.HasValue)
					income.Where (o => o.Date < Filter.RestrictEndDate.Value.AddDays (1));
				if (Filter.RestrictEmployee != null)
					income.Where (o => o.Employee == Filter.RestrictEmployee);

				var incomeList = income
					.JoinQueryOver (() => incomeAlias.Employee, () => employeeAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver (() => incomeAlias.Casher, () => casherAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver (() => incomeAlias.IncomeCategory, () => incomeCategoryAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver (() => incomeAlias.ExpenseCategory, () => expenseCategoryAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList (list => list
					.Select (() => incomeAlias.Id).WithAlias (() => resultAlias.Id)
						.Select (() => incomeAlias.Date).WithAlias (() => resultAlias.Date)
						.Select (() => incomeAlias.Money).WithAlias (() => resultAlias.Money)
						.Select (() => incomeAlias.Description).WithAlias (() => resultAlias.Description)
						.Select (() => employeeAlias.Name).WithAlias (() => resultAlias.EmployeeName)
						.Select (() => employeeAlias.LastName).WithAlias (() => resultAlias.EmployeeSurname)
						.Select (() => employeeAlias.Patronymic).WithAlias (() => resultAlias.EmployeePatronymic)
						.Select (() => casherAlias.Name).WithAlias (() => resultAlias.CasherName)
						.Select (() => casherAlias.LastName).WithAlias (() => resultAlias.CasherSurname)
						.Select (() => casherAlias.Patronymic).WithAlias (() => resultAlias.CasherPatronymic)
						.Select (Projections.SqlFunction("COALESCE", NHibernateUtil.String
							, Projections.Property(() => incomeCategoryAlias.Name)
							, Projections.Property(() => expenseCategoryAlias.Name)
						)).WithAlias (() => resultAlias.Category)
					)
					.TransformUsing (Transformers.AliasToBean<CashDocumentsVMNode> ())
					.List<CashDocumentsVMNode> ();
				
				incomeList.ToList ().ForEach (i => i.DocTypeEnum = CashDocumentType.Income);
				result.AddRange (incomeList);
			}
		
			if (Filter.RestrictIncomeCategory == null && (Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == CashDocumentType.Expense)) {
				var expense = UoW.Session.QueryOver<Expense> (() => expenseAlias);

				if (Filter.RestrictExpenseCategory != null)
					expense.Where (i => i.ExpenseCategory == Filter.RestrictExpenseCategory);
				if(Filter.RestrictStartDate.HasValue)
					expense.Where (o => o.Date >= Filter.RestrictStartDate.Value);
				if(Filter.RestrictEndDate.HasValue)
					expense.Where (o => o.Date < Filter.RestrictEndDate.Value.AddDays (1));
				if (Filter.RestrictEmployee != null)
					expense.Where (o => o.Employee == Filter.RestrictEmployee);

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
					.TransformUsing (Transformers.AliasToBean<CashDocumentsVMNode> ())
					.List<CashDocumentsVMNode> ();

				expenseList.ToList ().ForEach (i => i.DocTypeEnum = CashDocumentType.Expense);
				result.AddRange (expenseList);
			}

			if (Filter.RestrictIncomeCategory == null && (Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == CashDocumentType.AdvanceReport)) {
				var advanceReport = UoW.Session.QueryOver<AdvanceReport> (() => advanceReportAlias);

				if (Filter.RestrictExpenseCategory != null)
					advanceReport.Where (i => i.ExpenseCategory == Filter.RestrictExpenseCategory);
				if(Filter.RestrictStartDate.HasValue)
					advanceReport.Where (o => o.Date >= Filter.RestrictStartDate.Value);
				if(Filter.RestrictEndDate.HasValue)
					advanceReport.Where (o => o.Date < Filter.RestrictEndDate.Value.AddDays (1));
				if (Filter.RestrictEmployee != null)
					advanceReport.Where (o => o.Accountable == Filter.RestrictEmployee);

				var advanceReportList = advanceReport
					.JoinQueryOver (() => advanceReportAlias.Accountable, () => employeeAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver (() => advanceReportAlias.Casher, () => casherAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinQueryOver (() => advanceReportAlias.ExpenseCategory, () => expenseCategoryAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.SelectList (list => list
						.Select (() => advanceReportAlias.Id).WithAlias (() => resultAlias.Id)
						.Select (() => advanceReportAlias.Date).WithAlias (() => resultAlias.Date)
						.Select (() => advanceReportAlias.Money).WithAlias (() => resultAlias.Money)
						.Select (() => advanceReportAlias.Description).WithAlias (() => resultAlias.Description)
						.Select (() => employeeAlias.Name).WithAlias (() => resultAlias.EmployeeName)
						.Select (() => employeeAlias.LastName).WithAlias (() => resultAlias.EmployeeSurname)
						.Select (() => employeeAlias.Patronymic).WithAlias (() => resultAlias.EmployeePatronymic)
						.Select (() => casherAlias.Name).WithAlias (() => resultAlias.CasherName)
						.Select (() => casherAlias.LastName).WithAlias (() => resultAlias.CasherSurname)
						.Select (() => casherAlias.Patronymic).WithAlias (() => resultAlias.CasherPatronymic)
						.Select (() => expenseCategoryAlias.Name).WithAlias (() => resultAlias.Category)
					)
					.TransformUsing (Transformers.AliasToBean<CashDocumentsVMNode> ())
					.List<CashDocumentsVMNode> ();

				advanceReportList.ToList ().ForEach (i => i.DocTypeEnum = CashDocumentType.AdvanceReport);
				result.AddRange (advanceReportList);
			}

			SetItemsSource (result.OrderByDescending (d => d.Date).ToList ());
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<CashDocumentsVMNode>.Create ()
			//.AddColumn ("Номер").SetDataProperty (node => node.Id.ToString())
			.AddColumn ("Тип документа").SetDataProperty (node => node.DocTypeString)
			.AddColumn ("Дата").SetDataProperty (node => node.DateString)
			.AddColumn ("Сотрудник").SetDataProperty (node => node.EmployeeString)
			.AddColumn ("Статья").SetDataProperty (node => node.Category)
			.AddColumn ("Сумма").AddTextRenderer (node => CurrencyWorks.GetShortCurrencyString (node.Money))
			.AddColumn ("Кассир").SetDataProperty (node => node.CasherString)
			.AddColumn ("Основание").SetDataProperty (node => node.Description)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			return true;
		}

		#endregion

		public CashDocumentsVM (CashDocumentsFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public CashDocumentsVM () : this(UnitOfWorkFactory.CreateWithoutRoot ())
		{
			CreateRepresentationFilter = () => new CashDocumentsFilter(UoW);
		}

		public CashDocumentsVM (IUnitOfWork uow) : base (
			typeof(Income),
			typeof(Expense),
			typeof(AdvanceReport))
		{
			this.UoW = uow;
		}
	}

	public class CashDocumentsVMNode
	{

		public int Id { get; set; }

		public CashDocumentType DocTypeEnum { get; set; }

		public string DocTypeString { get { return DocTypeEnum.GetEnumTitle(); } }

		public DateTime Date { get; set; }

		public string DateString { get { return  Date.ToShortDateString (); } }

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

