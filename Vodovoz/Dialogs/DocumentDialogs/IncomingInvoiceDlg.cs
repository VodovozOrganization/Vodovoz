using System;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class IncomingInvoiceDlg : OrmGtkDialogBase<IncomingInvoice>
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		public IncomingInvoiceDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<IncomingInvoice> ();
			ConfigureDlg ();
		}

		public IncomingInvoiceDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<IncomingInvoice> (id);
			ConfigureDlg ();
		}

		public IncomingInvoiceDlg (IncomingInvoice sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			tableInvoice.DataSource = subjectAdaptor;
			referenceContractor.SubjectType = typeof(Counterparty);
			referenceWarehouse.SubjectType = typeof(Warehouse);
			referenceContractor.ItemsCriteria = Session.CreateCriteria<Counterparty> ()
				.Add (Restrictions.Eq ("CounterpartyType", CounterpartyType.supplier));
			incominginvoiceitemsview1.DocumentUoW = UoWGeneric;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<IncomingInvoice> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем входящую накладную...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}
	}
}

