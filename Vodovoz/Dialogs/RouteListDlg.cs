using System;
using Vodovoz.Domain;
using QSOrmProject;
using NLog;
using QSValidation;
using Vodovoz.Repository;
using Vodovoz.Domain.Orders;

namespace Vodovoz
{
	public partial class RouteListDlg : OrmGtkDialogBase<RouteList>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public RouteListDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<RouteList> ();
			UoWGeneric.Root.Date = DateTime.Now;
			ConfigureDlg ();
		}

		public RouteListDlg (RouteList sub) : this(sub.Id) {}

		public RouteListDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			subjectAdaptor.Target = UoWGeneric.Root;
			dataRouteList.DataSource = subjectAdaptor;
			enumStatus.DataSource = subjectAdaptor;
			referenceCar.SubjectType = typeof(Car);
			referenceDriver.SubjectType = typeof(Employee);
			referenceDriver.Sensitive = false;
			entryNumber.Sensitive = false;
			//TODO Сделать удаление
			buttonDelete.Sensitive = false;
		}

		public override bool Save() {
			var valid = new QSValidator<RouteList> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем маршрутный лист...");
			UoWGeneric.Save ();
			logger.Info ("Ok");
			return true;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			OrmReference SelectDialog = new OrmReference (typeof(Order), 
				UoWGeneric, 
				OrderRepository.GetAcceptedOrdersForDateQueryOver(UoWGeneric.Root.Date)
					.GetExecutableQueryOver(UoWGeneric.Session).RootCriteria);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanEdit;
			SelectDialog.ObjectSelected += (s, ea) => {
				if (ea.Subject != null) {
					UoWGeneric.Root.AddOrder (ea.Subject as Order);
				}
			};
			TabParent.AddSlaveTab (this, SelectDialog);
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}
	}
}

