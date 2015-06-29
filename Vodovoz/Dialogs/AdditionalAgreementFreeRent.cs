using System.Collections.Generic;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementFreeRent : OrmGtkDialogBase<FreeRentAgreement>
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		public AdditionalAgreementFreeRent (CounterpartyContract contract)
		{
			this.Build ();
			UoWGeneric = FreeRentAgreement.Create (contract);
			ConfigureDlg ();
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
			var identifiers = new List<object> ();
			foreach (DeliveryPoint d in UoWGeneric.Root.Contract.Counterparty.DeliveryPoints)
				identifiers.Add (d.Id);
			referenceDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPoint.ItemsCriteria = Session.CreateCriteria<DeliveryPoint> ()
				.Add (Restrictions.In ("Id", identifiers));
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
			return true;
		}
	}
}

