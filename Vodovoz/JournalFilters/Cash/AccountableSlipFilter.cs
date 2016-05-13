using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AccountableSlipFilter : Gtk.Bin, IRepresentationFilter, IAccountableSlipsFilter
	{
		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
			}
		}

		public AccountableSlipFilter (IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public AccountableSlipFilter ()
		{
			this.Build ();

			yentryExpense.ItemsQuery = Repository.Cash.CategoryRepository.ExpenseCategoriesQuery ();
			yentryAccountable.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeQuery ();
			yentryAccountable.SetObjectDisplayFunc<Employee> (e => e.ShortName);
		}

		#region IReferenceFilter implementation

		public event EventHandler Refiltered;

		void OnRefiltered ()
		{
			if (Refiltered != null)
				Refiltered (this, new EventArgs ());
		}

		#endregion

		public ExpenseCategory RestrictExpenseCategory {
			get { return yentryExpense.Subject as ExpenseCategory;}
			set { yentryExpense.Subject = value;
				yentryExpense.Sensitive = false;
			}
		}

		public Employee RestrictAccountable {
			get { return yentryAccountable.Subject as Employee;}
			set { yentryAccountable.Subject = value;
				yentryAccountable.Sensitive = false;
			}
		}

		public DateTime? RestrictStartDate {
			get { return dateperiod.StartDateOrNull; }
			set {
				dateperiod.StartDateOrNull = value;
				dateperiod.Sensitive = false;
			}
		}

		public DateTime? RestrictEndDate {
			get { return dateperiod.EndDateOrNull; }
			set {
				dateperiod.EndDateOrNull = value;
				dateperiod.Sensitive = false;
			}
		}

		public decimal? RestrictDebt {
			get { return null;
			}
		}

		protected void OnYentryAccountableChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnYentryExpenseChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnDateperiodPeriodChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}
	}
}

