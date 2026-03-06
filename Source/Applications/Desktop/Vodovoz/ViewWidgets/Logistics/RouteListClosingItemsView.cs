using Autofac;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Infrastructure;

namespace Vodovoz
{
	[ToolboxItem(true)]
	public partial class RouteListClosingItemsView : WidgetOnTdiTabBase
	{
		private IEmployeeRepository _employeeRepository;
		private IRouteColumnRepository _routeColumnRepository;

		private readonly Gdk.Pixbuf _undoIcon;
		private readonly Gdk.Pixbuf _jumpToIcon;

		private Menu menu;

		private int goodsColumnsCount = -1;

		public event RowActivatedHandler OnClosingItemActivated;
		public event EventHandler<int> BottlesReturnedEdited;

		public RouteListClosingItemsView()
		{
			ResolveDependencies();
			Build();
			ConfigureMenu();
			ytreeviewItems.EnableGridLines = TreeViewGridLines.Both;
			_undoIcon = Stetic.IconLoader.LoadIcon(this, "gtk-undo", IconSize.Menu);
			_jumpToIcon = Stetic.IconLoader.LoadIcon(this, "gtk-jump-to", IconSize.Menu);
		}

		[Obsolete("Должен быть удален при разрешении проблем с контейнером")]
		private void ResolveDependencies()
		{
			_employeeRepository = ScopeProvider.Scope.Resolve<IEmployeeRepository>();
			_routeColumnRepository = ScopeProvider.Scope.Resolve<IRouteColumnRepository>();
		}
		
		public GenericObservableList<RouteListItem> Items { get; set; }

		private IList<RouteColumn> _columnsInfo;

		private IList<RouteColumn> columnsInfo {
			get {
				if(_columnsInfo == null && UoW != null)
				{
					_columnsInfo = _routeColumnRepository.ActiveColumns(UoW);
				}

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

		public IUnitOfWork UoW { get; set; }
		public bool CanChangeDriverSurcharge = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_change_driver_surcharge");
		private Gdk.Pixbuf transferFromIcon;

		protected Gdk.Pixbuf TransferFromIcon {
			get {
				if(transferFromIcon == null) {
					transferFromIcon = _undoIcon;
				}
				return transferFromIcon;
			}
		}

		private Gdk.Pixbuf transferInIcon;

		protected Gdk.Pixbuf TransferInIcon {
			get {
				if(transferInIcon == null) {
					transferInIcon = _jumpToIcon;
				}
				return transferInIcon;
			}
		}

		private RouteList routeList;
		public RouteList RouteList {
			get {
				return routeList;
			}
			set {
				if(routeList == value)
				{
					return;
				}

				routeList = value;
				if(routeList.Addresses == null)
				{
					routeList.Addresses = new List<RouteListItem>();
				}

				Items = routeList.ObservableAddresses;

				routeList.ObservableAddresses.ListChanged += Items_ListChanged;

				UpdateColumns();

				ytreeviewItems.ItemsDataSource = Items;
				ytreeviewItems.Reorderable = true;
			}
		}

		private bool isEditing;

		public bool IsEditing {
			get {
				return isEditing;
			}

			set {
				isEditing = value;
				ytreeviewItems.Sensitive = isEditing;
			}
		}

		private void Items_ListChanged(object aList)
		{
			UpdateColumns();
		}

		private void ChangeVisibility ()
		{
			string[] columsToChange = {"Залоги за\n бутыли", "Залоги за\n оборудование","  З/П\nводителя", " доплата\nводителя", "    З/П\nэкспедитора", " доплата\nэкспедитора",};
			
			foreach (var columnName in columsToChange) {
				var column = ytreeviewItems.Columns.FirstOrDefault(x => x.Title == columnName);
				if (column == null)
				{
					continue;
				}

				column.Visible = ColumsVisibility;
			}
		}

		private void UpdateColumns ()
		{
			var goodsColumns = Items.SelectMany (i => i.GoodsByRouteColumns.Keys).Distinct ().ToArray ();
			if (goodsColumnsCount == goodsColumns.Length)
			{
				return;
			}

			goodsColumnsCount = goodsColumns.Length;

			var config = ColumnsConfigFactory.Create<RouteListItem>()
				.AddColumn("Еж.\n №").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Order.DailyNumber.ToString())
				.AddColumn("Заказ").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Order.Id.ToString())
				.AddColumn("УПД")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Order
						.OrderDocuments.FirstOrDefault(d => (d.Type == OrderDocumentType.UPD 
					                                     || d.Type == OrderDocumentType.SpecialUPD)
					                                    && node.Order.Id == d.Order.Id
					                                    && d.DocumentOrganizationCounter != null) != null 
						? node.Order.OrderDocuments.FirstOrDefault(d => (d.Type == OrderDocumentType.UPD 
						                                                 || d.Type == OrderDocumentType.SpecialUPD)
						                                                 && node.Order.Id == d.Order.Id
						                                                 && d.DocumentOrganizationCounter != null).DocumentOrganizationCounter.DocumentNumber
						: "")
					.AddPixbufRenderer(x => GetRowIcon(x))
				.AddColumn("Адрес").HeaderAlignment(0.5f).AddTextRenderer(node =>
					node.Order.DeliveryPoint == null
						? string.Empty
						: $"{node.Order.DeliveryPoint.Street} д.{node.Order.DeliveryPoint.Building}")
				.AddColumn("Опл.").HeaderAlignment(0.5f).AddTextRenderer(node => node.Order.PaymentType.GetEnumShortTitle())
				.AddColumn("Доставка за час")
					.AddToggleRenderer(x => x.Order.IsFastDelivery).Editing(false);
			if (columnsInfo != null)
			{
				foreach (var column in columnsInfo)
				{
					if (!goodsColumns.Contains(column.Id))
					{
						continue;
					}

					int id = column.Id;
					config.AddColumn(column.Name).HeaderAlignment(0.5f)
						.AddTextRenderer()
							.AddSetter((cell,node)=>cell.Markup = WaterToClientString(node,id));
				}
			}
			var colorWhite = GdkColors.PrimaryBase;
			var colorRed = GdkColors.DangerBase;
			var colorDarkRed = GdkColors.DarkRed;
			var colorLightBlue = GdkColors.InfoBase;
			var colorYellow = GdkColors.WarningBase;

			config
				.AddColumn("Пустых\nбутылей").HeaderAlignment(0.5f).EnterToNextCell()
					.AddNumericRenderer(node => node.BottlesReturned)
					.EditedEvent(OnBottlesReturnedEdited)
				.AddSetter((cell, node) => cell.Editable = (node.IsDelivered()))
				.Adjustment(new Adjustment(0, 0, 100000, 1, 1, 1))
						.AddSetter(EmptyBottleCellSetter)
				.AddColumn("Бутылей по\n акции").HeaderAlignment(0.5f).EnterToNextCell()
					.AddNumericRenderer(node => node.Order.BottlesByStockActualCount)
				.AddColumn("Залоги за\n бутыли").HeaderAlignment(0.5f).EnterToNextCell()
					.AddNumericRenderer(node => node.BottleDepositsCollected)
				.AddColumn("Залоги за\n оборудование").HeaderAlignment(0.5f).EnterToNextCell()
					.AddNumericRenderer(node => node.EquipmentDepositsCollected)
				.AddColumn("Доп.\n(нал.)").HeaderAlignment(0.5f).EnterToNextCell()
					.AddNumericRenderer(node => node.ExtraCash).Editing()
					.Adjustment(new Adjustment(0, -1000000, 1000000, 1, 1, 1))
				.AddColumn("№ оплаты")
					.AddTextRenderer(node => node.Order.OnlinePaymentNumber.ToString())
						.AddSetter((cell, node) => cell.Editable = 
							(node.Order.PaymentType == PaymentType.Terminal || node.Order.PaymentType == PaymentType.PaidOnline) &&
							node.Status != RouteListItemStatus.Transfered &&
							node.Status != RouteListItemStatus.Canceled &&
							node.Status != RouteListItemStatus.Overdue)
						.EditedEvent(YTreeViewItemsOnlineOrderEdited)
				.AddColumn("Итого\n(нал.)").HeaderAlignment(0.5f).EnterToNextCell()
					.AddNumericRenderer(node => node.TotalCash)
				.AddColumn("Итого\n(терм.)").HeaderAlignment(0.5f).EnterToNextCell()
					.AddNumericRenderer(node => 
						(node.Status != RouteListItemStatus.Transfered && 
						node.Order.PaymentType == PaymentType.Terminal) ? node.Order.OrderSum : 0)
				.AddColumn ("Комментарий\nкассира")
					.AddTextRenderer (node => node.CashierComment).EditedEvent(CommentCellEdited).Editable()
				// Комментарий менеджера ответственного за водительский телефон
				.AddColumn("Вод. телефон").HeaderAlignment(0.5f)
					.SetTag("DriverNumber")
					.AddTextRenderer(node => node.Order.CommentManager)
				.AddColumn("Комментарий к заказу").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Order.Comment)
				.AddColumn("  З/П\nводителя").HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.DriverWage)						
				.AddColumn (" доплата\nводителя").HeaderAlignment (0.5f)
					.AddNumericRenderer (node => node.DriverWageSurcharge)
						.Adjustment (new Adjustment (0, -100000, 100000, 10, 100, 10))
						.AddSetter ((cell, node) => cell.Editable = node.IsDelivered() && CanChangeDriverSurcharge)
				.AddColumn("    З/П\nэкспедитора").HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.ForwarderWage)
				.AddColumn("Доп. оборудование\n     клиенту").HeaderAlignment(0.5f)
					.AddTextRenderer()
						.AddSetter((cell,node)=>cell.Markup=ToClientString(node))
				.AddColumn("Доп. оборуд.\n от клиента").HeaderAlignment(0.5f)
					.AddTextRenderer()
						.AddSetter((cell,node)=>cell.Markup=FromClientString(node))
				.AddColumn("Тип переноса").HeaderAlignment(0.5f)
					.AddTextRenderer(item => item.GetTransferText(true))
				.AddColumn("Чужой район\n для водит.").HeaderAlignment(0.5f)
					.AddToggleRenderer(item => item.IsDriverForeignDistrict)
						.Editing(false)

				.AddColumn("").AddTextRenderer()
				.RowCells()
				.AddSetter<CellRenderer>((cell, node) =>
				{
					var color = colorWhite;
					if(!node.IsDelivered()) {
						color = colorRed;
					} else { 
						var itemChanged = node.Order.OrderItems
					                               .Where(item => !item.Nomenclature.IsSerial)
					                               .Where(item => Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
					                               .Any(item => !item.IsDelivered);
						var equipmentChanged = node.Order.OrderEquipments.Any(eq => !eq.IsFullyDelivered);
						if(itemChanged || equipmentChanged) {
							color = colorLightBlue;
						}
					}
					if(node.Status == RouteListItemStatus.Transfered) { 
						color = colorYellow;
					}
					cell.CellBackgroundGdk = color;
					//Выделение цветом ячейки с заказом, если валидация этого заказа не пройдет при сохранении
					if(!node.AddressIsValid) { 
						var column = ytreeviewItems.Columns.FirstOrDefault(x => x.Title == "Заказ");
						if(column != null) {
							var renderer = column.CellRenderers.FirstOrDefault(x => x == cell);
							if(renderer != null) { 
								renderer.CellBackgroundGdk = colorDarkRed;
							}
						}
					}
				});

			ytreeviewItems.ColumnsConfig = config.Finish();
		}

		private void OnBottlesReturnedEdited(object o, EditedArgs args)
		{
			bool isNumeric = int.TryParse(args.NewText, out int bottlesReturned);

			if(!isNumeric)
			{
				return;
			}

			BottlesReturnedEdited(o, bottlesReturned);
		}

		private void YTreeViewItemsOnlineOrderEdited(object o, EditedArgs args) {
			var node = ytreeviewItems.GetSelectedObject<RouteListItem>();

			var isNumber = int.TryParse(args.NewText, out var res);

			if (node != null && isNumber && res > 0) {
				node.Order.OnlinePaymentNumber = res;
			}
		}

		private Gdk.Pixbuf GetRowIcon(RouteListItem item)
		{
			if (item.Status == RouteListItemStatus.Transfered)
			{
				return TransferFromIcon;
			}

			if (item.WasTransfered)
			{
				return TransferInIcon;
			}

			return null;
		}

		private void CommentCellEdited (object o, EditedArgs args)
		{
			var node = ytreeviewItems.YTreeModel.NodeAtPath(new TreePath(args.Path)) as RouteListItem;

			node.CashierCommentAuthor = _employeeRepository.GetEmployeeForCurrentUser(UoW);

			if (node.CashierCommentCreateDate.HasValue)
			{
				node.CashierCommentLastUpdate = DateTime.Now.Date;
			}
			else
			{
				node.CashierCommentCreateDate = DateTime.Now.Date;
			}
		}

		private void EmptyBottleCellSetter(Gamma.GtkWidgets.Cells.NodeCellRendererSpin<RouteListItem> cell, RouteListItem node)
		{
			if(!ytreeviewItems.Sensitive) {
				cell.Weight = 700;
				return;
			}
			if(node.DriverBottlesReturned.HasValue) {
				if(node.BottlesReturned == node.DriverBottlesReturned) {
					cell.ForegroundGdk = GdkColors.SuccessText;
				} else {
					cell.ForegroundGdk = GdkColors.InfoText;
				}
			} else {
				cell.ForegroundGdk = GdkColors.PrimaryText;
			}
		}

		public string WaterToClientString(RouteListItem item, int id)
		{
			var planned = item.GetGoodsAmountForColumn(id);
			var actual = item.GetGoodsActualAmountForColumn(id);
			var formatString = actual < planned
				? "<b>{0:N0}</b>({1:N0})" 
				: "<b>{0:N0}</b>";
			return string.Format(formatString, actual, planned-actual);
		}

		public string ToClientString(RouteListItem item)
		{
			var stringParts = new List<string>();
			if(item.PlannedCoolersToClient > 0) {
				var formatString = item.CoolersToClient < item.PlannedCoolersToClient
						? "Кулеры:<b>{0}</b>({1})"
						: "Кулеры:<b>{0}</b>";
				var coolerString = string.Format(formatString, item.CoolersToClient, item.PlannedCoolersToClient - item.CoolersToClient);
				stringParts.Add(coolerString);
			}
			if(item.PlannedPumpsToClient > 0) {
				var formatString = item.PumpsToClient < item.PlannedPumpsToClient
						? "Помпы:<b>{0}</b>({1})"
						: "Помпы:<b>{0}</b>";
				var coolerString = string.Format(formatString,
					item.PumpsToClient,
					item.PlannedPumpsToClient - item.PumpsToClient
				);
				stringParts.Add(coolerString);
			}
			if(item.UncategorisedEquipmentToClient > 0) {
				var formatString = item.UncategorisedEquipmentToClient < item.PlannedUncategorisedEquipmentToClient
						? "Другое:<b>{0}</b>({1})"
						: "Другое:<b>{0}</b>";
				var coolerString = string.Format(formatString,
					item.UncategorisedEquipmentToClient,
					item.PlannedUncategorisedEquipmentToClient - item.UncategorisedEquipmentToClient
				);
				stringParts.Add(coolerString);
			}

			foreach(var orderItem in item.Order.OrderItems) {
				if(new NomenclatureCategory[] {
					NomenclatureCategory.additional,
					NomenclatureCategory.equipment,
					NomenclatureCategory.spare_parts
					}.Contains(orderItem.Nomenclature.Category)) {
					stringParts.Add(
						orderItem.IsDelivered
						? string.Format("{0}:<b>{1}</b>", orderItem.Nomenclature.Name, orderItem.ActualCount ?? 0)
						: string.Format("{0}:{1}({2:-0})", orderItem.Nomenclature.Name, orderItem.ActualCount ?? 0, orderItem.Count - orderItem.ActualCount ?? 0)
					);
				}

			}

			//Оборудование не из товаров
			var equipList = item.Order.OrderEquipments
					.Where(x => //x.Nomenclature.Category != NomenclatureCategory.water && 
			               x.Direction == Core.Domain.Orders.Direction.Deliver);
			foreach(OrderEquipment orderEquip in equipList) {
				stringParts.Add(orderEquip.IsFullyDelivered
									? string.Format("{0}:{1} ", orderEquip.NameString, orderEquip.ActualCount ?? 0)
				                	: string.Format("{0}:{1}({2:-0})", orderEquip.NameString, orderEquip.ActualCount ?? 0, orderEquip.Count - orderEquip.ActualCount ?? 0)
				                );
			}

			//Если это старый заказ со старой записью оборудования в виде строки, то выводим только его
			if(!string.IsNullOrWhiteSpace(item.Order.ToClientText)) {
				return item.Order.ToClientText;
			}

			return string.Join(",", stringParts);
		}	

		public string FromClientString(RouteListItem item)
		{
			var stringParts = new List<string>();

			if(item.PlannedCoolersFromClient > 0) {
				var formatString = item.CoolersFromClient < item.PlannedCoolersFromClient
						? "Кулеры:<b>{0}</b>({1})"
						: "Кулеры:<b>{0}</b>";
				var coolerString = string.Format(formatString,
					item.CoolersFromClient,
					item.PlannedCoolersFromClient - item.CoolersFromClient
				);
				stringParts.Add(coolerString);
			}
			if(item.PlannedPumpsFromClient > 0) {
				var formatString = item.PumpsFromClient < item.PlannedPumpsFromClient
						? "Помпы:<b>{0}</b>({1})"
						: "Помпы:<b>{0}</b>";
				var pumpString = string.Format(formatString,
					item.PumpsFromClient,
					item.PlannedPumpsFromClient - item.PumpsFromClient
				);
				stringParts.Add(pumpString);
			}

			//Оборудование не из товаров
			var equipList = item.Order.OrderEquipments
			                    .Where(x => x.OrderItem == null
			                           //&& x.Nomenclature.Category != NomenclatureCategory.water
			                           && x.Direction == Core.Domain.Orders.Direction.PickUp);
			foreach(var orderEquip in equipList) {
				stringParts.Add(orderEquip.IsFullyDelivered
									? string.Format("{0}:{1} ", orderEquip.NameString, orderEquip.ActualCount ?? 0)
									: string.Format("{0}:{1}({2:-0})", orderEquip.NameString, orderEquip.ActualCount ?? 0, orderEquip.Count - orderEquip.ActualCount ?? 0)
								);
			}

			//Если это старый заказ со старой записью оборудования в виде строки, то выводим только его
			if(!string.IsNullOrWhiteSpace(item.Order.FromClientText)) {
				return item.Order.FromClientText;
			}

			return string.Join(",", stringParts);
		}

		private Dictionary<PopupMenuAction, MenuItem> menuItems;
		private string textToCopy;

		public void ConfigureMenu()
		{
			menuItems = new Dictionary<PopupMenuAction, MenuItem>();
			menu = new Menu();

			Clipboard clipboard = Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));

			var copy = new MenuItem(PopupMenuAction.CopyCell.GetEnumTitle());
			menuItems.Add(PopupMenuAction.CopyCell, copy);
			copy.Activated += (s, args) => {
				if(!string.IsNullOrWhiteSpace(textToCopy))
				{
					clipboard.Text = textToCopy;
				}
			};
			copy.Visible = false;

			var openReturns = new MenuItem(PopupMenuAction.OpenClosingOrder.GetEnumTitle());
			openReturns.Activated += (s, args) => OnClosingItemActivated(this,null);
			menuItems.Add(PopupMenuAction.OpenClosingOrder, openReturns);

			var openOrder = new MenuItem(PopupMenuAction.OpenOrder.GetEnumTitle());
			openOrder.Activated += (s, args) => {
				var dlg = new OrderDlg(GetSelectedRouteListItem().Order);
				dlg.Show();
				MyTab.TabParent.AddTab(dlg, MyTab);
			};
			menuItems.Add(PopupMenuAction.OpenOrder, openOrder);

			var copyOrderId = new MenuItem(PopupMenuAction.CopyOrderId.GetEnumTitle());
			copyOrderId.Activated += (s, args) =>
			{
				var selectedOrderId = GetSelectedRouteListItem().Order?.Id;

				if(selectedOrderId != null)
				{
					GetClipboard(Gdk.Selection.Clipboard).Text = selectedOrderId.Value.ToString();
				}
			};
			menuItems.Add(PopupMenuAction.CopyOrderId, copyOrderId);

			foreach(var item in menuItems) {
				menu.Append(item.Value);
				item.Value.Show();
			}
		}

		private void OnYtreeviewItemsRowActivated(object sender, RowActivatedArgs args)
		{
			if(args.Column.Title == "Заказ" && (
				GetSelectedRouteListItem().Status == RouteListItemStatus.Transfered
				|| GetSelectedRouteListItem ().WasTransfered
			))
			{
				MessageDialogHelper.RunInfoDialog (GetSelectedRouteListItem ().GetTransferText());
				return;
			}
			OnClosingItemActivated(sender, args);
		}

		public RouteListItem GetSelectedRouteListItem()
		{
			return ytreeviewItems.GetSelectedObject() as RouteListItem;
		}

		[GLib.ConnectBefore]
		protected void OnYtreeviewItemsButtonPressEvent(object o, ButtonPressEventArgs buttonPressArgs)
		{
			if (buttonPressArgs.Event.Button == 3)
			{
				int x = (int)buttonPressArgs.Event.X;
				int y = (int)buttonPressArgs.Event.Y;
				ytreeviewItems.GetPathAtPos(x, y, out TreePath path, out TreeViewColumn column);
				ytreeviewItems.Model.GetIter(out TreeIter iter, path);
				var driverCol = ytreeviewItems.ColumnsConfig.GetColumnsByTag("DriverNumber").Where(c => c == column).ToArray();

				menuItems[PopupMenuAction.CopyCell].Visible = false;
				if(driverCol.Any() && ytreeviewItems.YTreeModel.NodeFromIter(iter) is RouteListItem node) {
					textToCopy = node.Order.CommentManager;
					menuItems[PopupMenuAction.CopyCell].Visible = true;
				}
				var selectedItem = GetSelectedRouteListItem();
				if (selectedItem != null)
				{
					menu.Popup();
				}
			}
		}

		public override void Destroy()
		{
			_employeeRepository = null;
			_routeColumnRepository = null;
			base.Destroy();
		}

		public enum PopupMenuAction
		{
			[Display(Name = "Копировать ячейку")]
			CopyCell,
			[Display(Name = "Открыть закрытие заказа")]
			OpenClosingOrder,
			[Display(Name = "Открыть заказ")]
			OpenOrder,
			[Display(Name = "Копировать номер заказа")]
			CopyOrderId
		}
	}		
}
