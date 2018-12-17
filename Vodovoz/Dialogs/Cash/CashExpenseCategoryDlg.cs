using System;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Domain.Cash;

namespace Vodovoz
{
	
	public partial class CashExpenseCategoryDlg : QS.Dialog.Gtk.EntityDialogBase<ExpenseCategory>
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public CashExpenseCategoryDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<ExpenseCategory> ();
			ConfigureDialog ();
		}

		public CashExpenseCategoryDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<ExpenseCategory> (id);
			ConfigureDialog ();
		}

		public CashExpenseCategoryDlg (ExpenseCategory sub): this(sub.Id) {}


		protected void ConfigureDialog(){
			yentryName.Binding
				.AddBinding (Entity, e => e.Name, (widget) => widget.Text)
				.InitializeFromSource ();

			yentryParent.SubjectType = typeof(ExpenseCategory);
			yentryParent.Binding.AddBinding(Entity, e => e.Parent, w => w.Subject).InitializeFromSource();

			yenumTypeDocument.ItemsEnum = typeof(ExpenseInvoiceDocumentType);
			yenumTypeDocument.Binding.AddBinding(Entity, e => e.ExpenseDocumentType, w => w.SelectedItem).InitializeFromSource();
		}

		public override bool Save ()
		{
			var valid = new QSValidation.QSValidator<ExpenseCategory> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем статью расхода...");
			UoWGeneric.Save ();
			return true;
		}

	}
}

