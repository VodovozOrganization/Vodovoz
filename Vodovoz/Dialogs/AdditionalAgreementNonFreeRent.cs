using System.Collections.Generic;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementNonFreeRent : OrmGtkDialogBase<NonfreeRentAgreement>
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		public AdditionalAgreementNonFreeRent (CounterpartyContract contract)
		{
			this.Build ();
			UoWGeneric = NonfreeRentAgreement.Create (contract);
			ConfigureDlg ();
		}

		public AdditionalAgreementNonFreeRent (NonfreeRentAgreement sub) : this (sub.Id)
		{
		}

		public AdditionalAgreementNonFreeRent (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<NonfreeRentAgreement> (id);
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
			dataAgreementType.Text = UoWGeneric.Root.Contract.Number + " - А";

			paidrentpackagesview1.ParentReference = new OrmParentReference (Session, UoWGeneric.Root, "Equipment");
			paidrentpackagesview1.DailyRent = false;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<NonfreeRentAgreement> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем доп. соглашение...");
			//personsView.SaveChanges ();
			UoWGeneric.Save ();
			logger.Info ("Ok");
			return true;
		}
	}
}

