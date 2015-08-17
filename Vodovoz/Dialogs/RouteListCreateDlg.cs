using System;
using Vodovoz.Domain;
using QSOrmProject;
using NLog;
using QSValidation;
using Gtk;
using Vodovoz.Domain.Logistic;

namespace Vodovoz
{
	public partial class RouteListCreateDlg : OrmGtkDialogBase<RouteList>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public RouteListCreateDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<RouteList> ();
			UoWGeneric.Root.Date = DateTime.Now;
			ConfigureDlg ();
		}

		public RouteListCreateDlg (RouteList sub) : this (sub.Id)
		{
		}

		public RouteListCreateDlg (int id)
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

			referenceDriver.ItemsQuery = Repository.EmployeeRepository.DriversQuery ();
			referenceDriver.PropertyMapping<RouteList> (r => r.Driver);
			referenceDriver.SetObjectDisplayFunc<Employee> (r => r.FullName);

			dataentryForwarder.ItemsQuery = Repository.EmployeeRepository.ForwarderQuery ();
			dataentryForwarder.PropertyMapping<RouteList> (r => r.Forwarder);

			speccomboShift.Mappings = Entity.GetPropertyName (r => r.Shift);
			speccomboShift.ColumnMappings = PropertyUtil.GetName<DeliveryShift> (s => s.Name);
			speccomboShift.ItemsDataSource = Repository.Logistics.DeliveryShiftRepository.ActiveShifts (UoW);

			referenceDriver.Sensitive = false;
			enumStatus.Sensitive = false;

			createroutelistitemsview1.RouteListUoW = UoWGeneric;

			buttonAccept.Visible = (UoWGeneric.Root.Status == RouteListStatus.New || UoWGeneric.Root.Status == RouteListStatus.Ready);
			if (UoWGeneric.Root.Status == RouteListStatus.Ready) {
				var icon = new Image ();
				icon.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-edit", IconSize.Menu);
				buttonAccept.Image = icon;
				buttonAccept.Label = "Редактировать";
				IsEditable ();
			}
		}

		public override bool Save ()
		{
			var valid = new QSValidator<RouteList> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем маршрутный лист...");
			UoWGeneric.Save ();
			logger.Info ("Ok");
			return true;
		}

		private void IsEditable (bool val = false)
		{
			enumStatus.Sensitive = speccomboShift.Sensitive = val;
			datepickerDate.Sensitive = referenceCar.Sensitive = val;
			spinPlannedDistance.Sensitive = spinPlannedDistance.Sensitive = val;
			createroutelistitemsview1.IsEditable (val);
		}

		protected void OnButtonAcceptClicked (object sender, EventArgs e)
		{

			if (UoWGeneric.Root.Status == RouteListStatus.New) {
				UoWGeneric.Root.Status = RouteListStatus.Ready;
				IsEditable ();
				var icon = new Image ();
				icon.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-edit", IconSize.Menu);
				buttonAccept.Image = icon;
				buttonAccept.Label = "Редактировать";
				return;
			}
			if (UoWGeneric.Root.Status == RouteListStatus.Ready) {
				UoWGeneric.Root.Status = RouteListStatus.New;
				IsEditable (true);
				var icon = new Image ();
				icon.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-edit", IconSize.Menu);
				buttonAccept.Image = icon;
				buttonAccept.Label = "Подтвердить";
				return;
			}
		}
	}
}

