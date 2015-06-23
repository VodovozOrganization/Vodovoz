using System;
using QSOrmProject;
using Vodovoz.Domain;
using NLog;
using QSValidation;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OrderDlg : OrmGtkDialogBase<Order>
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		public OrderDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Order> ();
			TabName = "Новый заказ";
			ConfigureDlg ();
		}

		public OrderDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Order> (id);
			ConfigureDlg ();
		}

		public OrderDlg (Order sub) : this (sub.Id)
		{
		}

		public void ConfigureDlg ()
		{
			subjectAdaptor.Target = UoWGeneric.Root;
			datatable1.DataSource = subjectAdaptor;
			datatable2.DataSource = subjectAdaptor;
			datatable3.DataSource = subjectAdaptor;
			notebook1.Page = 0;
			notebook1.ShowTabs = false;
			referenceClient.SubjectType = typeof(Counterparty);
			referenceDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			referenceDeliverySchedule.SubjectType = typeof(DeliverySchedule);
			referencePreviousOrder.SubjectType = typeof(Order);
		}

		public override bool Save ()
		{
			var valid = new QSValidator<Order> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем заказ...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}

		protected void OnToggleInformationToggled (object sender, EventArgs e)
		{
			if (toggleInformation.Active)
				notebook1.CurrentPage = 0;
		}

		protected void OnToggleGoodsToggled (object sender, EventArgs e)
		{
			if (toggleGoods.Active)
				notebook1.CurrentPage = 1;
		}

		protected void OnToggleEquipmentToggled (object sender, EventArgs e)
		{
			if (toggleEquipment.Active)
				notebook1.CurrentPage = 2;
		}

		protected void OnToggleServiceToggled (object sender, EventArgs e)
		{
			if (toggleService.Active)
				notebook1.CurrentPage = 3;
		}

		protected void OnToggleDocumentsToggled (object sender, EventArgs e)
		{
			if (toggleDocuments.Active)
				notebook1.CurrentPage = 4;
		}

		protected void OnCheckSelfDeliveryToggled (object sender, EventArgs e)
		{
			pickerDeliveryDate.Sensitive = referenceDeliverySchedule.Sensitive = !checkSelfDelivery.Active;
			labelDeliveryDate.Sensitive = labelDeliveryDate.Sensitive = !checkSelfDelivery.Active;
		}
	}
}

