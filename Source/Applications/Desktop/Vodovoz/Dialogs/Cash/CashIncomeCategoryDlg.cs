using System;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Validation;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Dialogs.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	[Obsolete("Не используется, можно удалять")]
	public partial class CashIncomeCategoryDlg : EntityDialogBase<IncomeCategory>
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public CashIncomeCategoryDlg()
		{
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<IncomeCategory>();
			ConfigureDialog();
		}

		public CashIncomeCategoryDlg(int id)
		{
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<IncomeCategory>(id);
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
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			logger.Info("Сохраняем статью дохода...");
			UoWGeneric.Save();
			return true;
		}
	}
}
