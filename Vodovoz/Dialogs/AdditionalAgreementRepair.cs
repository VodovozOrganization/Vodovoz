using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Repository;
using System;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementRepair : OrmGtkDialogBase<RepairAgreement>, IAgreementSaved
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		public event EventHandler<AgreementSavedEventArgs> AgreementSaved;

		public AdditionalAgreementRepair (CounterpartyContract contract)
		{
			this.Build ();
			UoWGeneric = RepairAgreement.Create (contract);
			ConfigureDlg ();
		}

		public AdditionalAgreementRepair (RepairAgreement sub) : this (sub.Id)
		{
		}

		public AdditionalAgreementRepair (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RepairAgreement> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			datatable1.DataSource = subjectAdaptor;
			entryAgreementNumber.IsEditable = true;
			dataAgreementType.Text = UoWGeneric.Root.Contract.Number + " - Т";
		}

		public override bool Save ()
		{
			var valid = new QSValidator<RepairAgreement> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем доп. соглашение...");
			UoWGeneric.Save ();
			if (AgreementSaved != null)
				AgreementSaved (this, new AgreementSavedEventArgs (UoWGeneric.Root));
			logger.Info ("Ok");
			return true;
		}
	}
}

