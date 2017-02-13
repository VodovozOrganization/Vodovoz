using System;
using QSOrmProject;
using Vodovoz.Domain.Logistic;
using NHibernate.Criterion;
using QSTDI;
using Gamma.GtkWidgets;
using Gamma.ColumnConfig;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using System.Linq;

namespace Vodovoz
{
	public partial class RouteListAddressesTransferringDlg : TdiTabBase, ITdiDialog, IOrmDialog
	{
		private IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public bool HasChanges { 
			get {
				return uow.HasChanges;
			}
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW
		{
			get
			{
				return uow;
			}
		}

		public object EntityObject {
			get {
				throw new NotImplementedException();
			}
		}

		#endregion

		#region Конструкторы

		public RouteListAddressesTransferringDlg()
		{
			this.Build();
			TabName = "Перенос адресов маршрутных листов";
			ConfigureDlg();
		}

		#endregion

		#region Методы

		private void ConfigureDlg()
		{
			yentryreferenceRLFrom.SubjectType = typeof(RouteList);
			yentryreferenceRLTo	 .SubjectType = typeof(RouteList);

			yentryreferenceRLFrom.ItemsQuery = QueryOver.Of<RouteList>()
				.Where(rl => rl.Status == RouteListStatus.EnRoute)
				.OrderBy(rl => rl.Date).Desc;
			
			yentryreferenceRLTo.ItemsQuery = QueryOver.Of<RouteList>()
				.Where(rl => rl.Status == RouteListStatus.EnRoute 
						  || rl.Status == RouteListStatus.InLoading
						  || rl.Status == RouteListStatus.New)
				.OrderBy(rl => rl.Date).Desc;

			yentryreferenceRLFrom.Changed += YentryreferenceRLFrom_Changed;
			yentryreferenceRLTo	 .Changed += YentryreferenceRLTo_Changed;

			//Для каждой TreeView нужен свой экземпляр ColumnsConfig
			ytreeviewRLFrom	.ColumnsConfig = GetColumnsConfig();
			ytreeviewRLTo	.ColumnsConfig = GetColumnsConfig();

			ytreeviewRLFrom .Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeviewRLTo	.Selection.Mode = Gtk.SelectionMode.Multiple;

			ytreeviewRLFrom .Selection.Changed += YtreeviewRLFrom_OnSelectionChanged;
			ytreeviewRLTo	.Selection.Changed += YtreeviewRLTo_OnSelectionChanged;
		}
		
		void YtreeviewRLFrom_OnSelectionChanged (object sender, EventArgs e)
		{
			CheckButtonSensetive();
		}

		void YtreeviewRLTo_OnSelectionChanged (object sender, EventArgs e)
		{
			CheckButtonSensetive();
		}

		private IColumnsConfig GetColumnsConfig ()
		{
			return ColumnsConfigFactory.Create<RouteListItemNode>()
				.AddColumn("Заказ")		 .AddTextRenderer	(node => node.Id)
				.AddColumn("Время")		 .AddTextRenderer	(node => node.Date)
				.AddColumn("Адрес")		 .AddTextRenderer	(node => node.Address)
				.AddColumn("Бутыли")	 .AddTextRenderer	(node => node.BottlesCount)
				.AddColumn("Статус")	 .AddEnumRenderer	(node => node.Status)
				.AddColumn("Комментарий").AddTextRenderer	(node => node.Comment)
				.Finish();
		}

		void YentryreferenceRLFrom_Changed (object sender, EventArgs e)
		{
			if (yentryreferenceRLFrom.Subject == null)
				return;

			CheckButtonSensetive();

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

			CheckButtonSensetive();

			RouteList routeList = yentryreferenceRLTo.Subject as RouteList;
			IList<RouteListItemNode> items = new List<RouteListItemNode>();
			foreach (var item in routeList.Addresses)
				items.Add(new RouteListItemNode{RouteListItem = item});
			ytreeviewRLTo.ItemsDataSource = items;
		}

		private void UpdateNodes()
		{
			YentryreferenceRLFrom_Changed(null, null);
			YentryreferenceRLTo_Changed(null, null);
		}
		
		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		public bool Save()
		{
			uow.Commit();
			return true;
		}

		public void SaveAndClose()
		{
		}
		
		protected void OnButtonTransferClicked (object sender, EventArgs e)
		{
			//Дополнительные проверки
			RouteList routeListTo 	= yentryreferenceRLTo.Subject as RouteList;
			RouteList routeListFrom = yentryreferenceRLFrom.Subject as RouteList;

			if (routeListTo == null || routeListFrom == null || routeListTo.Id == routeListFrom.Id)
				return;
			
			foreach (var row in ytreeviewRLFrom.GetSelectedObjects())
			{
				RouteListItem item = (row as RouteListItemNode)?.RouteListItem;
				logger.Debug("Проверка адреса с номером {0}", item?.Id.ToString() ?? "Неправильный адрес");

				if (item == null || item.Status == RouteListItemStatus.Transfered)
					continue;

				RouteListItem newItem = new RouteListItem(routeListTo, item.Order);
				routeListTo.Addresses.Add(newItem);
				uow.Save(newItem);

				item.TransferedTo = newItem;
				uow.Save(item);
			}
			UpdateNodes();
		}

		private void CheckButtonSensetive ()
		{
			int selectionFromCount = ytreeviewRLFrom.GetSelectedObjects().Count();
			bool routeListToIsSelected = yentryreferenceRLTo.Subject != null;

			buttonTransfer.Sensitive = selectionFromCount > 0 && routeListToIsSelected;
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			Save();
		}

		#endregion
	}

	public class RouteListItemNode {
		public string Id {
			get {return RouteListItem.Order.Id.ToString();}
		}

		public string Date {
			get {return RouteListItem.Order.DeliveryDate.Value.ToString("d");}
		}

		public string Address {
			get {return RouteListItem.Order.DeliveryPoint?.ShortAddress ?? "Нет адреса";}
		}

		public RouteListItemStatus Status {
			get {return RouteListItem.Status;}
		}

		public string Comment {
			get {return RouteListItem.Comment ?? "";}
		}

		public string BottlesCount {
			get {return RouteListItem.Order.OrderItems
					.Where(bot => bot.Nomenclature.Category == Vodovoz.Domain.Goods.NomenclatureCategory.water
						|| bot.Nomenclature.Category == Vodovoz.Domain.Goods.NomenclatureCategory.disposableBottleWater)
					.Sum(bot => bot.Count).ToString();}
		}
			
		public RouteListItem RouteListItem { get; set; }
	}
}

