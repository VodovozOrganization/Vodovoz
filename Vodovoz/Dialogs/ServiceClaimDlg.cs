using System;
using QSOrmProject;
using Vodovoz.Domain.Service;
using NLog;
using Vodovoz.Domain.Orders;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ServiceClaimDlg : OrmGtkDialogBase<ServiceClaim>
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		public ServiceClaimDlg (Order order)
		{
			this.Build ();
			UoWGeneric = ServiceClaim.Create (order);
			ConfigureDlg ();
		}

		public ServiceClaimDlg (ServiceClaim sub) : this (sub.Id)
		{
		}

		public ServiceClaimDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<ServiceClaim> (id);
			ConfigureDlg ();
		}

		void ConfigureDlg ()
		{
			subjectAdaptor.Target = UoWGeneric.Root;

			datatable1.DataSource = subjectAdaptor;
			enumPaymentType.DataSource = subjectAdaptor;
			enumStatus.DataSource = subjectAdaptor;

			referenceCounterparty.SubjectType = typeof(Counterparty);
			referenceDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			referenceEngineer.SubjectType = typeof(Employee);
			referenceEquipment.SubjectType = typeof(Equipment);
			referenceNomenclature.SubjectType = typeof(Nomenclature);
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save ()
		{
			var valid = new QSValidator<ServiceClaim> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем заявку на обслуживание...");
			UoWGeneric.Save ();
			logger.Info ("Ok");
			return true;

		}

		#endregion
	}
}

