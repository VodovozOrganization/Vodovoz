using System;
using System.Collections.Generic;
using System.Data.Bindings;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSTDI;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementRepair : OrmGtkDialogBase<RepairAgreement>
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger ();

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
			var identifiers = new List<object> ();
			foreach (DeliveryPoint d in UoWGeneric.Root.Contract.Counterparty.DeliveryPoints)
				identifiers.Add (d.Id);
			referenceDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPoint.ItemsCriteria = Session.CreateCriteria<DeliveryPoint> ()
				.Add (Restrictions.In ("Id", identifiers));
			dataAgreementType.Text = UoWGeneric.Root.Contract.Number + " - Т";
		}

		public override bool Save ()
		{
			var valid = new QSValidator<RepairAgreement> (UoWGeneric.Root);
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

