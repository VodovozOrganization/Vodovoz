using System;
using Vodovoz.Domain;
using QSOrmProject;
using NLog;
using QSValidation;
using Vodovoz.Repository;
using Vodovoz.Domain.Orders;
using System.Data.Bindings;
using Gtk.DataBindings;
using Gtk;
using QSTDI;
using Vodovoz.Domain.Logistic;

namespace Vodovoz
{
	public partial class CreateRouteListDlg : OrmGtkDialogBase<RouteList>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public CreateRouteListDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<RouteList> ();
			UoWGeneric.Root.Date = DateTime.Now;
			ConfigureDlg ();
		}

		public CreateRouteListDlg (RouteList sub) : this (sub.Id)
		{
		}

		public CreateRouteListDlg (int id)
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
			treeOrders.ItemsDataSource = UoWGeneric.Root.ObservableOrders;

			referenceCar.SubjectType = typeof(Car);
			referenceDriver.SubjectType = typeof(Employee);

			referenceDriver.Sensitive = false;
			entryNumber.Sensitive = false;
			buttonDelete.Sensitive = false;
			enumStatus.Sensitive = false;

			buttonAccept.Visible = (UoWGeneric.Root.Status == RouteListStatus.New || UoWGeneric.Root.Status == RouteListStatus.Ready);
			if (UoWGeneric.Root.Status == RouteListStatus.Ready) {
				var icon = new Image ();
				icon.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-edit", IconSize.Menu);
				buttonAccept.Image = icon;
				buttonAccept.Label = "Редактировать";
				IsEditable ();
			}
			treeOrders.ColumnMappingConfig = FluentMappingConfig<Order>.Create ()
				.AddColumn ("Номер").SetDataProperty (node => node.Id)
				.AddColumn ("Клиент").SetDataProperty (node => node.Client.Name)
				.AddColumn ("Адрес").SetDataProperty (node => node.DeliveryPoint.Point)
				.AddColumn ("Логистический район").SetDataProperty (node => node.DeliveryPoint.LogisticsArea == null ? 
					"Не указан" : 
					node.DeliveryPoint.LogisticsArea.Name)
				.RowCells ().AddSetter<CellRendererText> ((c, n) => c.Foreground = n.RowColor)
				.Finish ();

			treeOrders.Selection.Changed += (sender, e) => {
				buttonDelete.Sensitive = treeOrders.Selection.CountSelectedRows () > 0;
			};
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

		protected void AddOrder ()
		{
			OrmReference SelectDialog = new OrmReference (UoWGeneric, OrderRepository.GetAcceptedOrdersForDateQueryOver (UoWGeneric.Root.Date));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanEdit;
			SelectDialog.ObjectSelected += (s, ea) => {
				if (ea.Subject != null) {
					UoWGeneric.Root.AddOrder (ea.Subject as Order);
				}
			};
			TabParent.AddSlaveTab (this, SelectDialog);
		}

		public void AddOrdersFromRegion ()
		{
			OrmReference SelectDialog = new OrmReference (typeof(LogisticsArea), UoWGeneric);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanEdit;
			SelectDialog.ObjectSelected += (s, ea) => {
				if (ea.Subject != null) {
					foreach (Order order in OrderRepository.GetAcceptedOrdersForRegion(UoWGeneric, UoWGeneric.Root.Date, ea.Subject as LogisticsArea))
						UoWGeneric.Root.AddOrder (order);
				}
			};
			TabParent.AddSlaveTab (this, SelectDialog);
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			UoWGeneric.Root.RemoveOrder (treeOrders.GetSelectedObjects () [0] as Order);
		}

		protected void OnEnumbuttonAddOrderEnumItemClicked (object sender, EnumItemClickedEventArgs e)
		{
			AddOrderEnum choice = (AddOrderEnum)e.ItemEnum;
			switch (choice) {
			case AddOrderEnum.AddOne:
				AddOrder ();
				break;
			case AddOrderEnum.AddAllForRegion:
				AddOrdersFromRegion ();
				break;
			default:
				break;
			}
		}

		private void IsEditable (bool val = false)
		{
			enumStatus.Sensitive = entryNumber.Sensitive = val;
			datepickerDate.Sensitive = referenceCar.Sensitive = val;
			spinPlannedDistance.Sensitive = spinPlannedDistance.Sensitive = val;
			enumbuttonAddOrder.Sensitive = buttonDelete.Sensitive = val;
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

		protected void OnTreeOrdersRowActivated (object o, RowActivatedArgs args)
		{
			if (treeOrders.GetSelectedObjects ().GetLength (0) > 0) {
				ITdiDialog dlg = null;
				dlg = OrmMain.CreateObjectDialog (treeOrders.GetSelectedObjects () [0] as Order);
				TabParent.AddSlaveTab (this, dlg);
			}
		}
	}

}

