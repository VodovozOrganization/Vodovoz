using System;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Repository;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementWater : OrmGtkDialogBase<WaterSalesAgreement>, IAgreementSaved, IEditableDialog
	{
		public event EventHandler<AgreementSavedEventArgs> AgreementSaved;

		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		bool isEditable = true;

		public bool IsEditable { 
			get { return isEditable; } 
			set {
				isEditable = value;
				buttonSave.Sensitive = entryAgreementNumber.Sensitive = 
					referenceDeliveryPoint.Sensitive = dateIssue.Sensitive = dateStart.Sensitive = 
						checkIsFixedPrice.Sensitive = spinFixedPrice.Sensitive = value;
			} 
		}

		public AdditionalAgreementWater (CounterpartyContract contract)
		{
			this.Build ();
			UoWGeneric = WaterSalesAgreement.Create (contract);
			ConfigureDlg ();
		}

		public AdditionalAgreementWater (WaterSalesAgreement sub) : this (sub.Id)
		{
		}

		public AdditionalAgreementWater (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<WaterSalesAgreement> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			datatable1.DataSource = subjectAdaptor;
			entryAgreementNumber.IsEditable = true;
			referenceDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPoint.ItemsCriteria = DeliveryPointRepository
				.DeliveryPointsForCounterpartyQuery (UoWGeneric.Root.Contract.Counterparty)
				.GetExecutableQueryOver (UoWGeneric.Session).RootCriteria;
			dataAgreementType.Text = UoWGeneric.Root.Contract.Number + " - В";
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

