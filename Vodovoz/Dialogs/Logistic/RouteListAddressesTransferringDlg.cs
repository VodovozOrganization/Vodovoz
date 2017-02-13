using System;
using QSOrmProject;
using Vodovoz.Domain.Logistic;
using NHibernate.Criterion;
using QSTDI;
using Gamma.GtkWidgets;
using Gamma.ColumnConfig;
using System.Collections.Generic;
using Vodovoz.Domain.Client;

namespace Vodovoz
{
	public partial class RouteListAddressesTransferringDlg : TdiTabBase, ITdiDialog
	{
		private IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		private IColumnsConfig columnsConfig = ColumnsConfigFactory.Create<RouteListItemNode>()
			.AddColumn("Заказ")		 .AddTextRenderer(node => node.Id)
			.AddColumn("Время")		 .AddTextRenderer(node => node.Date)
			.AddColumn("Адрес")		 .AddTextRenderer(node => node.Address)
			.AddColumn("Статус")	 .AddEnumRenderer(node => node.Status)
			.AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
			.Finish();

		#region Конструкторы

		public RouteListAddressesTransferringDlg()
		{
			this.Build();
			TabName = "Перенос адресов маршрутных листов";
			ConfigureDlg();
		}

		#endregion

		private void ConfigureDlg()
		{
			yentryreferenceRLFrom.SubjectType = typeof(RouteList);
			yentryreferenceRLTo	 .SubjectType = typeof(RouteList);

			yentryreferenceRLFrom.ItemsQuery = QueryOver.Of<RouteList>()
				.Where(rl => rl.Status == RouteListStatus.EnRoute);
			
			yentryreferenceRLTo.ItemsQuery = QueryOver.Of<RouteList>()
				.Where(rl => rl.Status == RouteListStatus.EnRoute 
						  || rl.Status == RouteListStatus.InLoading
						  || rl.Status == RouteListStatus.New);

			yentryreferenceRLFrom.Changed += YentryreferenceRLFrom_Changed;
			yentryreferenceRLTo	 .Changed += YentryreferenceRLTo_Changed;

			ytreeviewRLFrom .ColumnsConfig = columnsConfig;
			ytreeviewRLTo	.ColumnsConfig = columnsConfig;

			ytreeviewRLFrom .Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeviewRLTo	.Selection.Mode = Gtk.SelectionMode.Multiple;
		}

		void YentryreferenceRLFrom_Changed (object sender, EventArgs e)
		{
			if (yentryreferenceRLFrom.Subject == null)
				return;

			RouteList routeList = yentryreferenceRLFrom.Subject as RouteList;
			IList<RouteListItemNode> items = new List<RouteListItemNode>();
			foreach (var item in routeList.Addresses)
				items.Add(new RouteListItemNode{RouteListItem = item});
			ytreeviewRLFrom.ItemsDataSource = items;
		}

		void YentryreferenceRLTo_Changed (object sender, EventArgs e)
		{
			if (yentryreferenceRLTo.Subject == null)
				return;

			RouteList routeList = yentryreferenceRLTo.Subject as RouteList;
			IList<RouteListItemNode> items = new List<RouteListItemNode>();
			foreach (var item in routeList.Addresses)
				items.Add(new RouteListItemNode{RouteListItem = item});
			ytreeviewRLTo.ItemsDataSource = items;
		}
		
		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		public bool Save()
		{
			return false;
		}

		public void SaveAndClose()
		{
		}

		public bool HasChanges {
			get {
				return false;
			}
		}
	}

	public class RouteListItemNode {
		public string Id {
			get {return RouteListItem.Order.Id.ToString();}
		}

		public string Date {
			get {return RouteListItem.Order.DeliveryDate.Value.ToString("d");}
		}

		public string Address {
			get {return RouteListItem.Order.DeliveryPoint.ShortAddress;}
		}

		public RouteListItemStatus Status {
			get {return RouteListItem.Status;}
		}

		public string Comment {
			get {return RouteListItem.Comment ?? "";}
		}

		public RouteListItem RouteListItem { get; set; }
	}
}

