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

		IList<RouteListItem> items;

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
				//UpdateNodes();

				items=RouteListUoW.Root.Addresses;
				foreach (RouteListItem routeListitem in items)
				{
					routeListitem.BottlesReturned = routeListitem.Order.BottlesReturn;
					routeListitem.DepositsCollected = routeListitem.Order.OrderDepositItems.Sum(depositItem => depositItem.Deposit);
					routeListitem.TotalPrice = routeListitem.Order.TotalSum;
				}


				RouteListUoW.Root.ObservableAddresses.ElementChanged += Items_ElementChanged;
				RouteListUoW.Root.ObservableAddresses.ListChanged += Items_ListChanged;

				UpdateColumns ();

				ytreeviewItems.ItemsDataSource = items;
				ytreeviewItems.Reorderable = true;
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
			logger.Info("Redundant UpdateNodes call");
			/*
			items = new List<RouteListItemClosingNode>();
			foreach (RouteListItem routeListitem in RouteListUoW.Root.ObservableAddresses)
			{
				items.Add(new RouteListItemClosingNode{
					item = routeListitem,
					BottlesReturned=routeListitem.Order.BottlesReturn,
					DepositsCollected=routeListitem.Order.OrderDepositItems.Sum(depositItem=>depositItem.Deposit)
				});
			}
			*/
		}

		private void UpdateColumns ()
		{
			var goodsColumns = items.SelectMany (i => i.GoodsByRouteColumns.Keys).Distinct ().ToArray ();
			if (goodsColumnsCount == goodsColumns.Length)
				return;

			goodsColumnsCount = goodsColumns.Length;

			var config = ColumnsConfigFactory.Create<RouteListItem>()
				.AddColumn("Заказ").AddTextRenderer(node => node.Order.Id.ToString())
				.AddColumn("Адрес").AddTextRenderer(node => String.Format("{0} д.{1}", node.Order.DeliveryPoint.Street, node.Order.DeliveryPoint.Building))
				.AddColumn("Время").AddTextRenderer(node => node.Order.DeliverySchedule == null ? "" : node.Order.DeliverySchedule.Name);
			
			foreach (var column in columnsInfo) {
				if (!goodsColumns.Contains (column.Id))
					continue;
				int id = column.Id;
				config.AddColumn (column.Name).AddTextRenderer (a => a.GetGoodsAmountForColumn (id).ToString ());
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
				config.RowCells ().AddSetter<CellRendererText> ((c, n) => c.Foreground = n.Order.RowColor)
				.Finish ();
		}			

		void Items_ElementChanged (object aList, int[] aIdx)
		{
			UpdateNodes();
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
			int bottlesReturnedTotal = items.Sum(item => item.BottlesReturned);
			decimal depositsCollectedTotal = items.Sum(item => item.DepositsCollected);
			decimal total = items.Sum(item => item.TotalPrice);
			labelTotal.Text = String.Format("Итого бутылей:{0}\tИтого залогов:{1}\tИтого сдано:{2}",bottlesReturnedTotal,depositsCollectedTotal,total);
		}			
	}

}

