using System;
using QSOrmProject;
using Vodovoz.Domain.Cash;
using QSValidation;

namespace Vodovoz
{
	public partial class AdvanceReportDlg : OrmGtkDialogBase<AdvanceReport>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public AdvanceReportDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<AdvanceReport>();
			ConfigureDlg ();
		}

		public AdvanceReportDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<AdvanceReport> (id);
			ConfigureDlg ();
		}

		public AdvanceReportDlg (AdvanceReport sub) : this (sub.Id) {}

		void ConfigureDlg()
		{
			yentryCasher.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeQuery ();
			yentryCasher.Binding.AddBinding (Entity, s => s.Casher, w => w.Subject).InitializeFromSource ();

			yentryEmploeey.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeQuery ();
			yentryEmploeey.Binding.AddBinding (Entity, s => s.Accountable, w => w.Subject).InitializeFromSource ();

			ydateDocument.Binding.AddBinding (Entity, s => s.Date, w => w.Date).InitializeFromSource ();

			comboCategory.ItemsList = Repository.Cash.CategoryRepository.IncomeCategories (UoW);
			comboCategory.Binding.AddBinding (Entity, s => s.ExpenseCategory, w => w.SelectedItem).InitializeFromSource ();

			yspinMoney.Binding.AddBinding (Entity, s => s.Money, w => w.ValueAsDecimal).InitializeFromSource ();

			ytextviewDescription.Binding.AddBinding (Entity, s => s.Description, w => w.Buffer.Text).InitializeFromSource ();
		}

		public override bool Save ()
		{
			var valid = new QSValidator<AdvanceReport> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем авансовый отчет...");
			try {
				UoWGeneric.Save();
			} catch (Exception ex) {
				logger.Error (ex, "Не удалось записать авансовый отчет.");
				QSProjectsLib.QSMain.ErrorMessage ((Gtk.Window)this.Toplevel, ex);
				return false;
			}
			logger.Info ("Ok");
			return true;

		}

	}
}

