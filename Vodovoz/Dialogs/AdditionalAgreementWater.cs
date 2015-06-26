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
	public partial class AdditionalAgreementWater : OrmGtkDialogBase<WaterSalesAgreement>
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger ();

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
			var identifiers = new List<object> ();
			foreach (DeliveryPoint d in UoWGeneric.Root.Contract.Counterparty.DeliveryPoints)
				identifiers.Add (d.Id);
			referenceDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPoint.ItemsCriteria = Session.CreateCriteria<DeliveryPoint> ()
				.Add (Restrictions.In ("Id", identifiers));
			dataAgreementType.Text = UoWGeneric.Root.Contract.Number + " - В";
			spinFixedPrice.Sensitive = currencylabel1.Sensitive = UoWGeneric.Root.IsFixedPrice;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<WaterSalesAgreement> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем доп. соглашение...");
			//personsView.SaveChanges ();
			UoWGeneric.Save ();
			logger.Info ("Ok");
			return true;
		}

		protected void OnCheckIsFixedPriceToggled (object sender, EventArgs e)
		{
			spinFixedPrice.Sensitive = currencylabel1.Sensitive = UoWGeneric.Root.IsFixedPrice;
		}
	}
}

