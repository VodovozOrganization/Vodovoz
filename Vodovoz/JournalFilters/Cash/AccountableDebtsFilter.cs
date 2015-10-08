using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Cash;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AccountableDebtsFilter : Gtk.Bin, IRepresentationFilter
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

		public AccountableDebtsFilter (IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public AccountableDebtsFilter ()
		{
			this.Build ();

			entryreferenceExpense.ItemsQuery = Repository.Cash.CategoryRepository.ExpenseCategoriesQuery ();
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
			get { return entryreferenceExpense.Subject as ExpenseCategory;}
			set { entryreferenceExpense.Subject = value;
				entryreferenceExpense.Sensitive = false;
			}
		}

		protected void OnEntryreferenceExpenseChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}

	}
}

