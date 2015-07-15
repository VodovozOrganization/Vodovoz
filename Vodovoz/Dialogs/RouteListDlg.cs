using System;
using Vodovoz.Domain;
using QSOrmProject;
using NLog;
using QSValidation;
using Vodovoz.Repository;
using Vodovoz.Domain.Orders;
using System.Data.Bindings;
using Gtk.DataBindings;

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
			treeOrders.ItemsDataSource = UoWGeneric.Root.ObservableOrders;

			referenceCar.SubjectType = typeof(Car);
			referenceDriver.SubjectType = typeof(Employee);

			//TODO Сделать удаление
			referenceDriver.Sensitive = false;
			entryNumber.Sensitive = false;
			buttonDelete.Sensitive = false;

			treeOrders.ColumnMappingConfig = FluentMappingConfig<Order>.Create ()
				.AddColumn ("Номер").SetDataProperty (node => node.Id)
				.AddColumn ("Клиент").SetDataProperty (node => node.Client.Name)
				.AddColumn ("Адрес").SetDataProperty (node => node.DeliveryPoint.Point)
				.Finish ();
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

		protected void AddOrder ()
		{
			OrmReference SelectDialog = new OrmReference (UoWGeneric, OrderRepository.GetAcceptedOrdersForDateQueryOver(UoWGeneric.Root.Date));
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
			throw new NotImplementedException ();
		}

		protected void OnEnumbuttonAddOrderEnumItemClicked (object sender, EnumItemClickedEventArgs e)
		{
			AddOrderEnum  choice = (AddOrderEnum)e.ItemEnum;
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
	}

	public enum AddOrderEnum {
		[ItemTitleAttribute("Один заказ")] AddOne,
		[ItemTitleAttribute("Все заказы для логистического района")]AddAllForRegion
	}
}

