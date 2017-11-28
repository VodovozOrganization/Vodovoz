using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListClosingItemsView : WidgetOnTdiTabBase
	{
		static Logger logger = LogManager.GetCurrentClassLogger();

		public event RowActivatedHandler OnClosingItemActivated;

		private Menu menu;

		public GenericObservableList<RouteListItem> Items { get; set; }

		private int goodsColumnsCount = -1;

		private IList<RouteColumn> _columnsInfo;

		private IList<RouteColumn> columnsInfo {
			get {
				if(_columnsInfo == null && UoW != null)
					_columnsInfo = Repository.Logistics.RouteColumnRepository.ActiveColumns(UoW);
				return _columnsInfo;
			}
		}

		private bool columsVisibility = true;

		public bool ColumsVisibility {
			get { return columsVisibility; }
			set {
				columsVisibility = value;
				ChangeVisibility();
			}
		}

		private decimal bottleDepositPrice;

		IUnitOfWork uow;
		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
#if !SHORT
				bottleDepositPrice = NomenclatureRepository.GetBottleDeposit(UoW).GetPrice(1);
#endif
			}
		}

		private decimal equipmentDepositPrice;

		Gdk.Pixbuf transferFromIcon;

		protected Gdk.Pixbuf TransferFromIcon {
			get {
				if(transferFromIcon == null) {
					transferFromIcon = Stetic.IconLoader.LoadIcon(this, "gtk-undo", IconSize.Menu);
				}
				return transferFromIcon;
			}
		}

		Gdk.Pixbuf transferInIcon;

		protected Gdk.Pixbuf TransferInIcon {
			get {
				if(transferInIcon == null) {
					transferInIcon = Stetic.IconLoader.LoadIcon(this, "gtk-jump-to", IconSize.Menu);
				}
				return transferInIcon;
			}
		}

		RouteList routeList;
		public RouteList RouteList {
			get {
				return routeList;
			}
			set {
				if(routeList == value)
					return;
				routeList = value;
				if(routeList.Addresses == null)
					routeList.Addresses = new List<RouteListItem>();
				Items = routeList.ObservableAddresses;

				routeList.ObservableAddresses.ListChanged += Items_ListChanged;
				RouteList.ObservableAddresses.ElementChanged += OnRouteListItemChanged;

				UpdateColumns();

				ytreeviewItems.ItemsDataSource = Items;
				ytreeviewItems.Reorderable = true;
			}
		}

		bool isEditing;

		public bool IsEditing {
			get {
				return isEditing;
			}

			set {
				isEditing = value;
				ytreeviewItems.Sensitive = isEditing;
			}
		}

		void Items_ListChanged(object aList)
		{
			UpdateColumns();
		}

		void OnRouteListItemChanged(object aList, int[] aIdx)
		{
#if !SHORT
			foreach (int id in aIdx)
			{
				if (bottleDepositPrice != 0 && RouteList.ObservableAddresses[id].DepositsCollected % bottleDepositPrice != 0)
				{
					var fullDepositsCount = Math.Truncate(RouteList.ObservableAddresses[id].DepositsCollected / bottleDepositPrice);
					RouteList.ObservableAddresses[id].DepositsCollected = fullDepositsCount * bottleDepositPrice;
				}
			}
#endif
		}

		private void ChangeVisibility ()
		{
			string[] columsToChange = {"Залоги за\n бутыли", "Залоги за\n оборудование","  З/П\nводителя", " доплата\nводителя", "    З/П\nэкспедитора", " доплата\nэкспедитора",};
			
			foreach (var columnName in columsToChange) {
				var column = ytreeviewItems.Columns.FirstOrDefault(x => x.Title == columnName);
				if (column == null)
					continue;
				column.Visible = ColumsVisibility;
			}
		}

		private void UpdateColumns ()
		{
			var goodsColumns = Items.SelectMany (i => i.GoodsByRouteColumns.Keys).Distinct ().ToArray ();
			if (goodsColumnsCount == goodsColumns.Length)
				return;

			goodsColumnsCount = goodsColumns.Length;

			var config = ColumnsConfigFactory.Create<RouteListItem>()
			    .AddColumn("Еж.\n №").HeaderAlignment(0.5f)
											 .AddTextRenderer(node => node.Order.DailyNumber1c.ToString())
				.AddColumn("Заказ").HeaderAlignment(0.5f)
			                                 .AddTextRenderer(node => node.Order.Id.ToString())
			                                 .AddPixbufRenderer(x => GetRowIcon(x))
				.AddColumn("Адрес").HeaderAlignment(0.5f).AddTextRenderer(node => node.Order.DeliveryPoint == null ? String.Empty : String.Format("{0} д.{1}", node.Order.DeliveryPoint.Street, node.Order.DeliveryPoint.Building))
				.AddColumn("Опл.").HeaderAlignment(0.5f).AddTextRenderer(node => node.Order.PaymentType.GetEnumShortTitle());
			
			if (columnsInfo != null)
			{
				foreach (var column in columnsInfo)
				{
					if (!goodsColumns.Contains(column.Id))
						continue;
					int id = column.Id;
					config.AddColumn(column.Name).HeaderAlignment(0.5f)
						.AddTextRenderer()
							.AddSetter((cell,node)=>cell.Markup = WaterToClientString(node,id));
				}
			}
			var colorBlack = new Gdk.Color (0, 0, 0);
			var colorBlue = new Gdk.Color (0, 0, 0xff);
			var colorWhite = new Gdk.Color(0xff, 0xff, 0xff);
			var colorRed = new Gdk.Color(0xee, 0x66, 0x66);
			var colorLightBlue = new Gdk.Color(0xbb, 0xbb, 0xff);
			var colorYellow = new Gdk.Color(0xb3, 0xb3, 0x00);
			config
//				.AddColumn("Предпол.\n пустых").HeaderAlignment(0.5f)
//					.AddTextRenderer(x => x.Order.BottlesReturn.ToString()).Sensitive(false)
				.AddColumn("Пустых\nбутылей").HeaderAlignment(0.5f).EnterToNextCell()
					.AddNumericRenderer(node => node.BottlesReturned)
						.AddSetter((cell, node) => cell.Editable = node.IsDelivered())
						.Adjustment(new Adjustment(0, 0, 100000, 1, 1, 1))
						.AddSetter(EmptyBottleCellSetter)
				.AddColumn("Залоги за\n бутыли").HeaderAlignment(0.5f).EnterToNextCell()
					.AddNumericRenderer(node => node.DepositsCollected)
						.Adjustment(new Adjustment(0, -100000, 100000, (double)bottleDepositPrice, (double)bottleDepositPrice, 1))
						.AddSetter((cell, node) => cell.Editable = node.IsDelivered())
						.AddSetter((cell, node) => {
					var expectedDeposits = (node.GetFullBottlesDeliveredCount()-node.BottlesReturned)*bottleDepositPrice;
					cell.ForegroundGdk = expectedDeposits!=node.DepositsCollected ? colorBlue : colorBlack;
				})
				.AddColumn("Залоги за\n оборудование").HeaderAlignment(0.5f).EnterToNextCell()
					.AddNumericRenderer(node => node.EquipmentDepositsCollected)
						.Adjustment(new Adjustment(0, -100000, 100000, (double)equipmentDepositPrice, (double)equipmentDepositPrice, 1))
						.AddSetter((cell, node) => cell.Editable = node.IsDelivered())
						 
				.AddColumn("Итого\n(нал.)").HeaderAlignment(0.5f).EnterToNextCell()
					.AddNumericRenderer(node => node.TotalCash)
						.AddSetter((cell, node) => cell.Editable = node.Order.PaymentType == PaymentType.cash && 
													node.IsDelivered())
						.AddSetter((cell,node)=>cell.Sensitive = node.Order.PaymentType == PaymentType.cash)
						.Adjustment(new Adjustment(0, -100000, 100000, 100, 100, 1))
				.AddColumn ("Комментарий\nкассира")
				.AddTextRenderer (node => node.CashierComment).EditedEvent (CommentCellEdited).Editable()
				.AddColumn("Вод. телефон").HeaderAlignment(0.5f)
					.AddTextRenderer()
						.AddSetter((cell, node) => cell.Markup = FromClientString(node))
						.AddTextRenderer(node => node.Order.CommentManager)
				.AddColumn("  З/П\nводителя").HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.DriverWage)						
				.AddColumn (" доплата\nводителя").HeaderAlignment (0.5f)
					.AddNumericRenderer (node => node.DriverWageSurcharge)
						.Adjustment (new Adjustment (0, -100000, 100000, 10, 100, 10))
						.AddSetter ((cell, node) => cell.Editable = node.IsDelivered ())
				.AddColumn("    З/П\nэкспедитора").HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.ForwarderWage)
     //           .AddColumn(" доплата\nэкспедитора").HeaderAlignment (0.5f)
					//.AddNumericRenderer (node => node.ForwarderWageSurcharge)
					//	.AddSetter ((cell, node) => cell.Editable = node.WithForwarder && node.IsDelivered())
					//	.AddSetter ((cell, node) => cell.Sensitive = node.WithForwarder)
					//	.Adjustment (new Adjustment (0, -100000, 100000, 100, 100, 1))
				.AddColumn("Доп. оборудование\n     клиенту").HeaderAlignment(0.5f)
					.AddTextRenderer()
						.AddSetter((cell,node)=>cell.Markup=ToClientString(node))
#if SHORT
						.AddTextRenderer(node => node.ToClientText).Editable()
#endif
				.AddColumn("Доп. оборуд.\n от клиента").HeaderAlignment(0.5f)
					.AddTextRenderer()
						.AddSetter((cell,node)=>cell.Markup=FromClientString(node))
#if SHORT
						.AddTextRenderer(node => node.FromClientText).Editable()
#endif

			   .AddColumn("Вод. телефон").HeaderAlignment(0.5f)
					.AddTextRenderer()
						.AddTextRenderer(node => node.Order.DriverCallType.ToString())

				.AddColumn("").AddTextRenderer()
				.RowCells()
				.AddSetter<CellRenderer>((cell, node) =>
				{
						var color = colorWhite;
						if (!node.IsDelivered())
							color = colorRed;
					else
					{
						var itemChanged = node.Order.OrderItems
							.Where(item => !item.Nomenclature.Serial)
							.Where(item => Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
							.Any(item => item.Count != item.ActualCount);
						var equipmentChanged = node.Order.OrderEquipments
							.Any(eq => !eq.Confirmed);
						if (itemChanged || equipmentChanged)
							color = colorLightBlue;
					}
						if(node.Status == RouteListItemStatus.Transfered)
							color = colorYellow;
					cell.CellBackgroundGdk = color;
				});

			ytreeviewItems.ColumnsConfig = config.Finish();
		}

		Gdk.Pixbuf GetRowIcon(RouteListItem item)
		{
			if (item.Status == RouteListItemStatus.Transfered)
				return TransferFromIcon;
			if (item.WasTransfered)
				return TransferInIcon;
			return null;
		}

		string GetTransferText(RouteListItem item) // Дубликат метода в RouteListItem, надо переделать метод вызова попапа и убрать.
		{
			if (item.Status == RouteListItemStatus.Transfered)
			{
				if(item.TransferedTo != null)
					return String.Format("Заказ был перенесен в МЛ №{0} водителя {1}.", 
				                     item.TransferedTo.RouteList.Id,
				                     item.TransferedTo.RouteList.Driver.ShortName
				                    );
				else
					return "ОШИБКА! Адрес имеет статус перенесенного в другой МЛ, но куда он перенесен не указано.";
			}
			if (item.WasTransfered) {
				var transferedFrom = Repository.Logistics.RouteListItemRepository.GetTransferedFrom (UoW, item);
				if (transferedFrom != null)
					return String.Format ("Заказ из МЛ №{0} водителя {1}.",
										 transferedFrom.RouteList.Id,
										 transferedFrom.RouteList.Driver.ShortName
										);
				else
					return "ОШИБКА! Адрес помечен как перенесенный из другого МЛ, но строка откуда он был перенесен не найдена.";
			}
			return null;
		}

		void CommentCellEdited (object o, EditedArgs args)
		{
			var node = ytreeviewItems.YTreeModel.NodeAtPath(new TreePath(args.Path)) as RouteListItem;

			node.CashierCommentAuthor = EmployeeRepository.GetEmployeeForCurrentUser(UoW);

			if (node.CashierCommentCreateDate.HasValue)
				node.CashierCommentLastUpdate = DateTime.Now.Date;
			else
				node.CashierCommentCreateDate = DateTime.Now.Date;
		}

		private void EmptyBottleCellSetter(Gamma.GtkWidgets.Cells.NodeCellRendererSpin<RouteListItem> cell, RouteListItem node)
		{
			if(!ytreeviewItems.Sensitive) {
				cell.Weight = 700; 
			}
			if(node.DriverBottlesReturned.HasValue)
			{
				if(node.BottlesReturned == node.DriverBottlesReturned)
					cell.Foreground = "Green";
				else
					cell.Foreground = "Blue";
			}
			else
				cell.Foreground = "Black";
		}
			
		public string WaterToClientString(RouteListItem item, int id)
		{
			var planned = item.GetGoodsAmountForColumn(id);
			var actual = item.GetGoodsActualAmountForColumn(id);
			var formatString = actual < planned
				? "<b>{0}</b>({1})" 
				: "<b>{0}</b>";
			return String.Format(formatString, actual, planned-actual);
		}

		public string ToClientString(RouteListItem item)
		{
			var stringParts = new List<string>();

			foreach(var orderItem in item.Order.OrderEquipments) {
				if(orderItem.Direction == Domain.Orders.Direction.Deliver) {
					stringParts.Add(string.Format("{0}:{1} ", orderItem.NameString, orderItem.Count));
				}
			}
			return String.Join(",", stringParts);
		}	

		public string FromClientString(RouteListItem item)
		{
			var stringParts = new List<string>();

			foreach(var orderItem in item.Order.OrderEquipments) {
				if(orderItem.Direction == Domain.Orders.Direction.PickUp)
				 {
					stringParts.Add(string.Format("{0}:{1} ", orderItem.NameString, orderItem.Count));
				 }
			}
			return String.Join(",", stringParts);
		}

		public RouteListClosingItemsView ()
		{
			this.Build ();
			ConfigureMenu();
			ytreeviewItems.EnableGridLines = TreeViewGridLines.Both;
		}

		public void ConfigureMenu()
		{
			menu = new Menu();

			var openReturns = new MenuItem("Открыть недовозы");
			openReturns.Activated += (s, args) =>
				{
					OnClosingItemActivated(this,null);
				};
			menu.Append(openReturns);
			openReturns.Show();

			var openOrder = new MenuItem("Открыть заказ");
			openOrder.Activated += (s, args) =>
				{
					var dlg = new OrderDlg(GetSelectedRouteListItem().Order);
					dlg.Show();
					MyTab.TabParent.AddTab(dlg, MyTab);
				};
			menu.Append(openOrder);
			openOrder.Show();
		}

		void OnYtreeviewItemsRowActivated(object sender, RowActivatedArgs args)
		{
			if(args.Column.Title == "Заказ" && (
				GetSelectedRouteListItem().Status == RouteListItemStatus.Transfered
				|| GetSelectedRouteListItem ().WasTransfered
			))
			{
				MessageDialogWorks.RunInfoDialog (GetTransferText (GetSelectedRouteListItem ()));
				return;
			}
			OnClosingItemActivated(sender, args);
		}

		public RouteListItem GetSelectedRouteListItem()
		{
			return (ytreeviewItems.GetSelectedObject() as RouteListItem);
		}

		[GLib.ConnectBefore]
		protected void OnYtreeviewItemsButtonPressEvent (object o, ButtonPressEventArgs buttonPressArgs)
		{
			if (buttonPressArgs.Event.Button == 3)
			{
				var selectedItem = GetSelectedRouteListItem();
				if (selectedItem != null)
				{
					menu.Popup();
				}
			}
		}
	}		
}