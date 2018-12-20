using System;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Dialogs.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashIncomeCategoryDlg : EntityDialogBase<IncomeCategory>
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public CashIncomeCategoryDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<IncomeCategory>();
			ConfigureDialog();
		}

		public CashIncomeCategoryDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<IncomeCategory>(id);
			ConfigureDialog();
		}

		public CashIncomeCategoryDlg(IncomeCategory sub) : this(sub.Id) { }


		protected void ConfigureDialog()
		{
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			yenumTypeDocument.ItemsEnum = typeof(IncomeInvoiceDocumentType);
			yenumTypeDocument.Binding.AddBinding(Entity, e => e.IncomeDocumentType, w => w.SelectedItem).InitializeFromSource();
		}

		public override bool Save()
		{
			var valid = new QSValidation.QSValidator<IncomeCategory>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			logger.Info("Сохраняем статью дохода...");
			UoWGeneric.Save();
			return true;
		}
	}
}
