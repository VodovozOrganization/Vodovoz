using System;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSProjectsLib;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModel
{
	public class AccountableDebtsVM : RepresentationModelWithoutEntityBase<AccountableDebtsVMNode>
	{
		public AccountableDebtsFilter Filter {
			get {
				return RepresentationFilter as AccountableDebtsFilter;
			}
			set { RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			
			AccountableDebtsVMNode resultAlias = null;
			Expense operationAddAlias = null;
			Income operationReturnAlias = null;
			AdvanceReport operationRemoveAlias = null;
			Employee employeeAlias = null;

			var subqueryAdd = QueryOver.Of<Expense>(() => operationAddAlias)
				.Where(() => operationAddAlias.Employee.Id == employeeAlias.Id && operationAddAlias.TypeOperation == ExpenseType.Advance)
				.Select (Projections.Sum<Expense> (o => o.Money));

			var subqueryReturn = QueryOver.Of<Income>(() => operationReturnAlias)
				.Where(() => operationReturnAlias.Employee.Id == employeeAlias.Id && operationReturnAlias.TypeOperation == IncomeType.Return)
				.Select (Projections.Sum<Income> (o => o.Money));

			var subqueryRemove = QueryOver.Of<AdvanceReport>(() => operationRemoveAlias)
				.Where(() => operationRemoveAlias.Accountable.Id == employeeAlias.Id)
				.Select (Projections.Sum<AdvanceReport> (o => o.Money));

			if(Filter.RestrictExpenseCategory != null)
			{
				subqueryAdd.Where (e => e.ExpenseCategory == Filter.RestrictExpenseCategory);
				subqueryReturn.Where (e => e.ExpenseCategory == Filter.RestrictExpenseCategory);
				subqueryRemove.Where (e => e.ExpenseCategory == Filter.RestrictExpenseCategory);
			}

			var stocklist = UoW.Session.QueryOver<Employee> (() => employeeAlias)
				.SelectList(list => list
					.SelectGroup(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
					.Select (() => employeeAlias.Name).WithAlias (() => resultAlias.EmployeeName)
					.Select (() => employeeAlias.LastName).WithAlias (() => resultAlias.EmployeeSurname)
					.Select (() => employeeAlias.Patronymic).WithAlias (() => resultAlias.EmployeePatronymic)
					.SelectSubQuery (subqueryAdd).WithAlias(() => resultAlias.Append)
					.SelectSubQuery (subqueryRemove).WithAlias(() => resultAlias.Removed)
					.SelectSubQuery (subqueryReturn).WithAlias(() => resultAlias.Returned)
				)
				.TransformUsing(Transformers.AliasToBean<AccountableDebtsVMNode>())
				.List<AccountableDebtsVMNode>().Where(r => r.Debt != 0).ToList ();

			SetItemsSource (stocklist);
		}

		IColumnsConfig treeViewConfig = ColumnsConfigFactory.Create<AccountableDebtsVMNode> ()
			.AddColumn("Имя сотрудника").SetDataProperty (node => node.AccountableName)
			.AddColumn ("Задолжность").SetDataProperty (node => node.DebtText)
			.RowCells ().AddSetter<Gtk.CellRendererText> ((c, n) => c.Foreground = n.RowColor)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get {
				return treeViewConfig;
			}
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			if (updatedSubject is Expense)
				return (updatedSubject as Expense).TypeOperation == ExpenseType.Advance;

			if (updatedSubject is Income)
				return (updatedSubject as Income).TypeOperation == IncomeType.Return;

			return true;
		}

		#endregion

		public AccountableDebtsVM (AccountableDebtsFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public AccountableDebtsVM () 
			: this(UnitOfWorkFactory.CreateWithoutRoot ()) 
		{}

		public AccountableDebtsVM (IUnitOfWork uow) : base(typeof(Employee), typeof(Income), typeof(Expense), typeof(AdvanceReport))
		{
			this.UoW = uow;
		}
	}
		
	public class AccountableDebtsVMNode
	{

		public int Id{ get; set;}

		public string EmployeeSurname { get; set; }
		public string EmployeeName { get; set; }
		public string EmployeePatronymic { get; set; }

		[UseForSearch]
		public string AccountableName { get{ return String.Format ("{0} {1} {2}", EmployeeSurname, EmployeeName, EmployeePatronymic);
			}}

		public decimal Append{ get; set;}

		public decimal Removed{ get; set;}

		public decimal Returned{ get; set;}

		public string DebtText { get { 
				return CurrencyWorks.GetShortCurrencyString (Debt);
		}}

		public decimal Debt { get{
				return Append - Removed - Returned;
			}}

		public string RowColor {
			get {
				return Debt < 0 ? "red" : "black";
			}
		}
	}
}

