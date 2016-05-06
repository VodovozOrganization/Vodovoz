using System;
using System.Collections.Generic;
using System.Globalization;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NHibernate;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModel
{
	public class OrdersVM : RepresentationModelEntityBase<Order, OrdersVMNode>
	{
		public OrdersFilter Filter {
			get {
				return RepresentationFilter as OrdersFilter;
			}
			set { RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			OrdersVMNode resultAlias = null;
			Order orderAlias = null;
			Nomenclature nomenclatureAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			DeliverySchedule deliveryScheduleAlias = null;

			var query = UoW.Session.QueryOver<Order> (() => orderAlias);

			if(Filter.RestrictStatus != null)
			{
				query.Where (o => o.OrderStatus == Filter.RestrictStatus);
			}

			if(Filter.RestrictSelfDelivery != null)
			{
				query.Where (o => o.SelfDelivery == Filter.RestrictSelfDelivery);
			}

			if(Filter.RestrictCounterparty != null)
			{
				query.Where (o => o.Client == Filter.RestrictCounterparty);
			}

			if(Filter.RestrictDeliveryPoint != null)
			{
				query.Where (o => o.DeliveryPoint == Filter.RestrictDeliveryPoint);
			}

			if(Filter.RestrictStartDate != null)
			{
				query.Where (o => o.DeliveryDate >= Filter.RestrictStartDate);
			}

			if(Filter.RestrictEndDate != null)
			{
				query.Where (o => o.DeliveryDate <= Filter.RestrictEndDate.Value.AddDays (1).AddTicks (-1));
			}

			if(Filter.ExceptIds!=null && Filter.ExceptIds.Length>0)
				query.Where(o => !NHibernate.Criterion.RestrictionExtensions.IsIn(o.Id, Filter.ExceptIds));

			var bottleCountSubquery = NHibernate.Criterion.QueryOver.Of<OrderItem>(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water)
				.Select(NHibernate.Criterion.Projections.Sum(() => orderItemAlias.Count));

			var result = query
				.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias (o => o.DeliverySchedule, () => deliveryScheduleAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver (o => o.Client, () => counterpartyAlias)
				.SelectList (list => list
					.Select (() => orderAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => orderAlias.DeliveryDate).WithAlias (() => resultAlias.Date)
					.Select (() => deliveryScheduleAlias.Name).WithAlias (() => resultAlias.DeliveryTime)
					.Select (() => orderAlias.OrderStatus).WithAlias (() => resultAlias.StatusEnum)
					.Select (() => counterpartyAlias.Name).WithAlias (() => resultAlias.Counterparty)
					.Select (() => deliveryPointAlias.City).WithAlias (() => resultAlias.City)
					.Select (() => deliveryPointAlias.Street).WithAlias (() => resultAlias.Street)
					.Select (() => deliveryPointAlias.Building).WithAlias (() => resultAlias.Building)
					.SelectSubQuery(bottleCountSubquery).WithAlias (() => resultAlias.BottleAmount)
				)
				.TransformUsing (Transformers.AliasToBean<OrdersVMNode> ())
				.List<OrdersVMNode> ();

			SetItemsSource (result);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig <OrdersVMNode>.Create ()
			.AddColumn ("Номер").SetDataProperty (node => node.Id.ToString())
			.AddColumn ("Дата").SetDataProperty (node => node.Date.ToString("d"))
			.AddColumn ("Время").SetDataProperty (node => node.DeliveryTime)
			.AddColumn ("Статус").SetDataProperty (node => node.StatusEnum.GetEnumTitle ())
			.AddColumn ("Бутыли").AddTextRenderer(node => node.BottleAmount.ToString())
			.AddColumn ("Клиент").SetDataProperty (node => node.Counterparty)
			.AddColumn ("Адрес").SetDataProperty (node => node.Address)
			.RowCells ().AddSetter<CellRendererText> ((c, n) => c.Foreground = n.RowColor)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		public override bool PopupMenuExist
		{
			get
			{
				return true;
			}
		}

		public override Gtk.Menu GetPopupMenu(RepresentationSelectResult[] selected)
		{
			lastMenuSelected = selected;

			Menu popupMenu = new Gtk.Menu();
			Gtk.MenuItem menuItemYandex = new MenuItem("Открыть на Yandex картах(координаты)");
			menuItemYandex.Activated += MenuItemYandex_Activated; 
			popupMenu.Add(menuItemYandex);
			Gtk.MenuItem menuItemYandexAddress = new MenuItem("Открыть на Yandex картах(адрес)");
			menuItemYandexAddress.Activated += MenuItemYandexAddress_Activated;
			popupMenu.Add(menuItemYandexAddress);
			Gtk.MenuItem menuItemOSM = new MenuItem("Открыть на карте OSM");
			menuItemOSM.Activated += MenuItemOSM_Activated;
			popupMenu.Add(menuItemOSM);
			return popupMenu;
		}

		#endregion

		private RepresentationSelectResult[] lastMenuSelected;

		void MenuItemOSM_Activated (object sender, EventArgs e)
		{
			foreach(var sel in lastMenuSelected)
			{
				var order = UoW.GetById<Order>(sel.EntityId);
				if (order.DeliveryPoint == null || order.DeliveryPoint.Latitude == null || order.DeliveryPoint.Longitude == null)
					continue;

				System.Diagnostics.Process.Start(String.Format(CultureInfo.InvariantCulture, "http://www.openstreetmap.org/#map=17/{1}/{0}", order.DeliveryPoint.Longitude, order.DeliveryPoint.Latitude));
			}
		}

		void MenuItemYandexAddress_Activated (object sender, EventArgs e)
		{
			foreach(var sel in lastMenuSelected)
			{
				var order = UoW.GetById<Order>(sel.EntityId);
				if (order.DeliveryPoint == null)
					continue;

				System.Diagnostics.Process.Start(
					String.Format(CultureInfo.InvariantCulture, 
						"https://maps.yandex.ru/?text={0} {1} {2}", 
						order.DeliveryPoint.City,
						order.DeliveryPoint.Street,
						order.DeliveryPoint.Building
					));
			}
		}

		void MenuItemYandex_Activated (object sender, EventArgs e)
		{
			foreach(var sel in lastMenuSelected)
			{
				var order = UoW.GetById<Order>(sel.EntityId);
				if (order.DeliveryPoint == null || order.DeliveryPoint.Latitude == null || order.DeliveryPoint.Longitude == null)
					continue;

				System.Diagnostics.Process.Start(String.Format(CultureInfo.InvariantCulture, "https://maps.yandex.ru/?ll={0},{1}&z=17", order.DeliveryPoint.Longitude, order.DeliveryPoint.Latitude));
			}
		}

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (Order updatedSubject)
		{
			return true;
		}

		#endregion

		public OrdersVM (OrdersFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public OrdersVM () : this(UnitOfWorkFactory.CreateWithoutRoot ())
		{
			CreateRepresentationFilter = () => new OrdersFilter(UoW);
		}

		public OrdersVM (IUnitOfWork uow) : base ()
		{
			this.UoW = uow;
		}
	}

	public class OrdersVMNode
	{
		[UseForSearch]
		public int Id { get; set; }

		public OrderStatus StatusEnum { get; set; }

		public DateTime Date { get; set; }
		public string DeliveryTime { get; set; }
		public int BottleAmount { get; set; }

		[UseForSearch]
		public string Counterparty { get; set; }

		public string City { get; set; }
		public string Street { get; set; }
		public string Building { get; set; }

		[UseForSearch]
		public string Address { get{ return String.Format("{0}, {1} д.{2}", City, Street, Building); } }

		public string RowColor {
			get {
				if (StatusEnum == OrderStatus.Canceled)
					return "grey";
				if (StatusEnum == OrderStatus.Closed)
					return "green";
				if (StatusEnum == OrderStatus.NotDelivered)
					return "blue";
				return "black";

			}
		}
	}
}