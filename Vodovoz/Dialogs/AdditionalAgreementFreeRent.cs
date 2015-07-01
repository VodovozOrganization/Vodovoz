using System;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Repository;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementFreeRent : OrmGtkDialogBase<FreeRentAgreement>, IAgreementSaved
	{
		public event EventHandler<AgreementSavedEventArgs> AgreementSaved;

		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		public AdditionalAgreementFreeRent (CounterpartyContract contract)
		{
			this.Build ();
			UoWGeneric = FreeRentAgreement.Create (contract);
			ConfigureDlg ();
		}

		public AdditionalAgreementFreeRent (CounterpartyContract contract, DeliveryPoint point) : this (contract)
		{
			UoWGeneric.Root.DeliveryPoint = point;
			referenceDeliveryPoint.Sensitive = false;
		}

		public AdditionalAgreementFreeRent (FreeRentAgreement sub) : this (sub.Id)
		{
		}

		public AdditionalAgreementFreeRent (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<FreeRentAgreement> (id);
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
			dataAgreementType.Text = UoWGeneric.Root.Contract.Number + " - Б";
			freerentpackagesview1.AgreementUoW = UoWGeneric;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<FreeRentAgreement> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем доп. соглашение...");
			UoWGeneric.Save ();
			logger.Info ("Ok");
			AgreementSaved (this, new AgreementSavedEventArgs (UoWGeneric.Root));
			return true;
		}
	}
}

