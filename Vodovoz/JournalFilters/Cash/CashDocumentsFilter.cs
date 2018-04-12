using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Cash;
using Gamma.Widgets;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	[OrmDefaultIsFiltered(true)]
	public partial class CashDocumentsFilter : Gtk.Bin, IRepresentationFilter
	{
		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				enumcomboDocumentType.ItemsEnum = typeof(CashDocumentType);
				entryEmployee.RepresentationModel = new ViewModel.EmployeesVM(new EmployeeFilter(uow: UoW, showFired: false));
			}
		}

		public CashDocumentsFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public CashDocumentsFilter()
		{
			this.Build();
			yentryIncome.ItemsQuery = Repository.Cash.CategoryRepository.IncomeCategoriesQuery();
			yentryExpense.ItemsQuery = Repository.Cash.CategoryRepository.ExpenseCategoriesQuery();

			//Последние 30 дней.
			dateperiodDocs.StartDateOrNull = DateTime.Today.AddDays(-30);
			dateperiodDocs.EndDateOrNull = DateTime.Today.AddDays(1);
		}

		#region IReferenceFilter implementation

		public event EventHandler Refiltered;

		void OnRefiltered()
		{
			if(Refiltered != null)
				Refiltered(this, new EventArgs());
		}

		#endregion

		public CashDocumentType? RestrictDocumentType {
			get { return enumcomboDocumentType.SelectedItem as CashDocumentType?; }
			set {
				enumcomboDocumentType.SelectedItem = value;
				enumcomboDocumentType.Sensitive = false;
			}
		}

		public ExpenseCategory RestrictExpenseCategory {
			get { return yentryExpense.Subject as ExpenseCategory; }
			set {
				yentryExpense.Subject = value;
				yentryExpense.Sensitive = false;
			}
		}

		public IncomeCategory RestrictIncomeCategory {
			get { return yentryIncome.Subject as IncomeCategory; }
			set {
				yentryIncome.Subject = value;
				yentryIncome.Sensitive = false;
			}
		}

		public Employee RestrictEmployee {
			get { return entryEmployee.Subject as Employee; }
			set {
				entryEmployee.Subject = value;
				entryEmployee.Sensitive = false;
			}
		}

		public DateTime? RestrictStartDate {
			get { return dateperiodDocs.StartDateOrNull; }
			set {
				dateperiodDocs.StartDateOrNull = value;
				dateperiodDocs.Sensitive = false;
			}
		}

		public DateTime? RestrictEndDate {
			get { return dateperiodDocs.EndDateOrNull; }
			set {
				dateperiodDocs.EndDateOrNull = value;
				dateperiodDocs.Sensitive = false;
			}
		}

		protected void OnEnumcomboDocumentTypeEnumItemSelected(object sender, ItemSelectedEventArgs e)
		{
			OnRefiltered();
		}

		protected void OnDateperiodDocsPeriodChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnYentryExpenseChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnYentryIncomeChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnEntryEmployeeChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}

