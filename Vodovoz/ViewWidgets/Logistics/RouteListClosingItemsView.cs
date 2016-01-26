using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using NLog;
using QSOrmProject;
using QSTDI;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository;
using QSProjectsLib;
using System.ComponentModel;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class RouteListClosingItemsView : WidgetOnTdiTabBase
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		public List<RouteListClosingItem> Items{ get; set; }
		//List<TotalReturnsNode> totalReturns;

		private int goodsColumnsCount = -1;

		private IList<RouteColumn> _columnsInfo;

		private IList<RouteColumn> columnsInfo {
			get {
				if (_columnsInfo == null)
					_columnsInfo = Repository.Logistics.RouteColumnRepository.ActiveColumns (RouteListUoW);
				return _columnsInfo;
			}
		}

		private IUnitOfWorkGeneric<RouteList> routeListUoW;

		public IUnitOfWorkGeneric<RouteList> RouteListUoW {
			get { return routeListUoW; }
			set {
				if (routeListUoW == value)
					return;
				routeListUoW = value;
				if (RouteListUoW.Root.Addresses == null)
					RouteListUoW.Root.Addresses = new List<RouteListItem> ();				
				UpdateNodes();						

				RouteListUoW.Root.ObservableAddresses.ElementChanged += Items_ElementChanged;
				RouteListUoW.Root.ObservableAddresses.ListChanged += Items_ListChanged;

				UpdateColumns ();

				ytreeviewItems.ItemsDataSource = Items;
				ytreeviewItems.Reorderable = true;

				foreach (var item in Items)
				{
					
				}					

				CalculateTotal ();
			}
		}

		void Items_ListChanged (object aList)
		{
			//UpdateNodes();
			UpdateColumns ();
		}

		private void UpdateNodes()
		{
			//logger.Info("Redundant UpdateNodes call");
			Items = new List<RouteListClosingItem>();
			foreach (RouteListItem routeListitem in RouteListUoW.Root.ObservableAddresses)
			{				
				Items.Add(new RouteListClosingItem(routeListitem));
				routeListitem.BottlesReturned = routeListitem.Order.BottlesReturn;
				routeListitem.DepositsCollected = routeListitem.Order.OrderDepositItems.Sum(depositItem => depositItem.Deposit);
			}

		}

		private void UpdateColumns ()
		{
			var goodsColumns = Items.SelectMany (i => i.RouteListItem.GoodsByRouteColumns.Keys).Distinct ().ToArray ();
			if (goodsColumnsCount == goodsColumns.Length)
				return;

			goodsColumnsCount = goodsColumns.Length;

			var config = ColumnsConfigFactory.Create<RouteListClosingItem>()
				.AddColumn("Заказ").AddTextRenderer(node => node.RouteListItem.Order.Id.ToString())
				.AddColumn("Адрес").AddTextRenderer(node => String.Format("{0} д.{1}", node.RouteListItem.Order.DeliveryPoint.Street, node.RouteListItem.Order.DeliveryPoint.Building))
				.AddColumn("Время").AddTextRenderer(node => node.RouteListItem.Order.DeliverySchedule == null ? "" : node.RouteListItem.Order.DeliverySchedule.Name);
			
			foreach (var column in columnsInfo) {
				if (!goodsColumns.Contains (column.Id))
					continue;
				int id = column.Id;
				config.AddColumn (column.Name).AddTextRenderer (a => a.RouteListItem.GetGoodsAmountForColumn (id).ToString ());
			}

			config
				.AddColumn("Пустых бутылей")
				.AddNumericRenderer(node => node.BottlesReturned).Editing(true).Adjustment(new Adjustment(0, 0, 100000, 1, 1, 1))
					.AddTextRenderer(node => "шт", false)
				.AddColumn("Залоги за бутыли")
				.AddNumericRenderer(node => node.DepositsCollected)
						.Editing(true)
						.Adjustment(new Adjustment(0, 0, 100000, 100, 100, 1))									
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)					
				.AddColumn("Итого")
					.AddNumericRenderer(node => node.TotalPrice)
						.Editing(true)
						.Adjustment(new Adjustment(0,0,100000,100,100,1))
				.AddColumn("").AddTextRenderer();

			ytreeviewItems.ColumnsConfig = 
				config.RowCells ().AddSetter<CellRendererText> ((c, n) => c.Foreground = n.RouteListItem.Order.RowColor)
				.Finish ();					
		}			

		void Items_ElementChanged (object aList, int[] aIdx)
		{			
			CalculateTotal ();
		}

		public RouteListClosingItemsView ()
		{
			this.Build ();
			ytreeviewItems.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			
		}						

		void CalculateTotal ()
		{
			int bottlesReturnedTotal = Items.Sum(item => item.RouteListItem.BottlesReturned);
			decimal depositsCollectedTotal = Items.Sum(item => item.RouteListItem.DepositsCollected);
			decimal total = Items.Sum(item => item.RouteListItem.TotalPrice);
			labelTotal.Text = String.Format("Итого бутылей:{0}\tИтого залогов:{1}\tИтого сдано:{2}",bottlesReturnedTotal,depositsCollectedTotal,total);
		}		

		void OnYtreeviewItemsRowActivated(object sender, RowActivatedArgs args)
		{
			var dlg = new OrderClosingView(ytreeviewItems.GetSelectedObject() as RouteListClosingItem);
			MyTab.TabParent.AddSlaveTab(MyTab, dlg);
		}


	}

	public class RouteListClosingItem
	{
		public RouteListItem RouteListItem{ get; set; }
		public List<OrderClosingItem> OrderClosingItems{ get; set;}

		public decimal DepositsCollected{
			get{
				return RouteListItem.DepositsCollected;
			}
			set{
				RouteListItem.DepositsCollected = value;
			}
		}

		public int BottlesReturned{
			get{
				return RouteListItem.BottlesReturned;
			}
			set{
				RouteListItem.BottlesReturned = value;
			}
		}

		public decimal TotalPrice{
			get{
				return RouteListItem.TotalPrice;
			}
			set{
				RouteListItem.TotalPrice = value;
			}
		}

		public RouteListClosingItem(RouteListItem routeListItem)
		{
			this.RouteListItem = routeListItem;
			OrderClosingItems = new List<OrderClosingItem>();		
			foreach (OrderItem orderItem in routeListItem.Order.OrderItems)
			{
				if (Nomenclature.GetCategoriesForShipment().Contains(orderItem.Nomenclature.Category))
				{
					OrderClosingItems.Add(new OrderClosingItem()
						{
							OrderItem = orderItem,
							Returned = false
						});
				}
			}
		}
	}

	public class OrderClosingItem
	{
		public OrderItem OrderItem{get;set;}
		public decimal Amount{ get; set; }
		public bool Returned{
			get{
				return Amount > 0;
			}
			set{
				Amount = value ? 1 : 0;
			}
		}
	}

	public class TotalReturnsNode{
		public int Id{get;set;}
		public NomenclatureCategory NomenclatureCategory{ get; set; }
		public int NomenclatureId{ get; set; }
		public string Name{get;set;}
		public decimal Amount{ get; set;}
		public bool Trackable{ get; set; }
		public string Serial{ get { 			
				if (Trackable) {
					return Id > 0 ? Id.ToString () : "(не определен)";
				} else
					return String.Empty;
			}
		}
		public bool Returned {
			get {
				return Amount > 0;
			}
			set {
				Amount = value ? 1 : 0;
			}
		}
	}

}

