using System;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain.Client;

namespace Vodovoz
{
	public partial class WaterAgreementDlg : OrmGtkDialogBase<WaterSalesAgreement>, IAgreementSaved, IEditableDialog
	{
		public event EventHandler<AgreementSavedEventArgs> AgreementSaved;

		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		bool isEditable = true;

		public bool IsEditable { 
			get { return isEditable; } 
			set {
				isEditable = value;
				buttonSave.Sensitive = 
					referenceDeliveryPoint.Sensitive = dateIssue.Sensitive = dateStart.Sensitive = 
						checkIsFixedPrice.Sensitive = spinFixedPrice.Sensitive = value;
			} 
		}

		public WaterAgreementDlg (CounterpartyContract contract)
		{
			this.Build ();
			UoWGeneric = WaterSalesAgreement.Create (contract);
			ConfigureDlg ();
		}

		public WaterAgreementDlg (CounterpartyContract contract, DateTime date) : this (contract)
		{
			UoWGeneric.Root.IssueDate = UoWGeneric.Root.StartDate = date;
		}

		public WaterAgreementDlg (WaterSalesAgreement sub) : this (sub.Id)
		{
		}

		public WaterAgreementDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<WaterSalesAgreement> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			datatable1.DataSource = subjectAdaptor;
			referenceDeliveryPoint.RepresentationModel = new ViewModel.DeliveryPointsVM (UoW, Entity.Contract.Counterparty);
			ylabelNumber.Binding.AddBinding(Entity, e => e.FullNumberText, w => w.LabelProp).InitializeFromSource();
			spinFixedPrice.Sensitive = currencylabel1.Sensitive = UoWGeneric.Root.IsFixedPrice;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<WaterSalesAgreement> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем доп. соглашение...");
			UoWGeneric.Save ();
			logger.Info ("Ok");
			if (AgreementSaved != null)
				AgreementSaved (this, new AgreementSavedEventArgs (UoWGeneric.Root));
			return true;
		}

		protected void OnCheckIsFixedPriceToggled (object sender, EventArgs e)
		{
			spinFixedPrice.Sensitive = currencylabel1.Sensitive = UoWGeneric.Root.IsFixedPrice;
		}
	}
}

