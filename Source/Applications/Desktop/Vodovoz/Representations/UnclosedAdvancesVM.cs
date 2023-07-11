﻿using System;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSProjectsLib;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;

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

		public override void UpdateNodes()
		{
			UnclosedAdvancesVMNode resultAlias = null;

			Employee employeeAlias = null;
			Employee casherAlias = null;

			Expense expenseAlias = null;
			FinancialExpenseCategory financialExpenseCategoryAlias = null;

			AdvanceClosing clousingAliace = null;

			var expense = UoW.Session.QueryOver<Expense>(() => expenseAlias)
				.Where(e => e.AdvanceClosed == false && e.TypeOperation == ExpenseType.Advance);

			if(Filter.RestrictExpenseCategory != null)
				expense.Where(i => i.ExpenseCategoryId == Filter.RestrictExpenseCategory.Id);
			if(Filter.RestrictAccountable != null)
				expense.Where(o => o.Employee == Filter.RestrictAccountable);
			if(Filter.RestrictStartDate != null)
				expense.Where(a => a.Date >= Filter.RestrictStartDate);
			if(Filter.RestrictEndDate != null)
				expense.Where(a => a.Date <= Filter.RestrictEndDate.Value.AddDays(1).AddTicks(-1));

			var subqueryClosed = QueryOver.Of<AdvanceClosing>(() => clousingAliace)
				.Where(() => clousingAliace.AdvanceExpense.Id == expenseAlias.Id)
				.Select(Projections.Sum<AdvanceClosing>(o => o.Money));

			var expenseList = expense
				.JoinEntityAlias(
						() => financialExpenseCategoryAlias,
						() => expenseAlias.ExpenseCategoryId == financialExpenseCategoryAlias.Id,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => expenseAlias.Employee, () => employeeAlias)
				.Left.JoinAlias(() => expenseAlias.Casher, () => casherAlias)
				.SelectList(list => list
				   .Select(() => expenseAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => expenseAlias.Date).WithAlias(() => resultAlias.Date)
				   .Select(() => expenseAlias.Money).WithAlias(() => resultAlias.Money)
				   .Select(() => expenseAlias.Description).WithAlias(() => resultAlias.Description)
				   .Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmployeeName)
				   .Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeSurname)
				   .Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
				   .Select(() => casherAlias.Name).WithAlias(() => resultAlias.CasherName)
				   .Select(() => casherAlias.LastName).WithAlias(() => resultAlias.CasherSurname)
				   .Select(() => casherAlias.Patronymic).WithAlias(() => resultAlias.CasherPatronymic)
				   .Select(() => financialExpenseCategoryAlias.Title).WithAlias(() => resultAlias.Category)
				   .SelectSubQuery(subqueryClosed).WithAlias(() => resultAlias.СloseMoney)
				)
				.TransformUsing(Transformers.AliasToBean<UnclosedAdvancesVMNode>())
				.List<UnclosedAdvancesVMNode>();

			SetItemsSource(expenseList.OrderByDescending(d => d.Date).ToList());
		}

		IColumnsConfig treeViewConfig = ColumnsConfigFactory.Create<UnclosedAdvancesVMNode>()
			//.AddColumn ("Номер").SetDataProperty (node => node.Id.ToString())
			.AddColumn("Дата").SetDataProperty(node => node.DateString)
			.AddColumn("Сотрудник").SetDataProperty(node => node.EmployeeString)
			.AddColumn("Статья").SetDataProperty(node => node.Category)
			.AddColumn("Получено").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Money))
			.AddColumn("Непогашено").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.UncloseMoney))
			.AddColumn("Кассир").SetDataProperty(node => node.CasherString)
			.AddColumn("Основание").SetDataProperty(node => node.Description)
			.Finish();

		public override IColumnsConfig ColumnsConfig {
			get { return treeViewConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(object updatedSubject)
		{
			return true;
		}

		#endregion

		public UnclosedAdvancesVM(UnclosedAdvancesFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public UnclosedAdvancesVM() : this(UnitOfWorkFactory.CreateWithoutRoot())
		{
			CreateRepresentationFilter = () => new UnclosedAdvancesFilter(UoW);
		}

		public UnclosedAdvancesVM(IUnitOfWork uow) : base(
			typeof(Expense))
		{
			this.UoW = uow;
		}
	}

	public class UnclosedAdvancesVMNode
	{

		public int Id { get; set; }

		public DateTime Date { get; set; }

		public string DateString { get { return Date.ToString(); } }

		public string EmployeeSurname { get; set; }
		public string EmployeeName { get; set; }
		public string EmployeePatronymic { get; set; }

		public string EmployeeString {
			get {
				return StringWorks.PersonNameWithInitials(EmployeeSurname, EmployeeName, EmployeePatronymic);
			}
		}

		public string CasherSurname { get; set; }
		public string CasherName { get; set; }
		public string CasherPatronymic { get; set; }

		public string CasherString {
			get {
				return StringWorks.PersonNameWithInitials(CasherSurname, CasherName, CasherPatronymic);
			}
		}

		public string Category { get; set; }

		public string Description { get; set; }

		public decimal Money { get; set; }

		public decimal СloseMoney { get; set; }

		public decimal UncloseMoney {
			get {
				return Money - СloseMoney;
			}
		}
	}
}

