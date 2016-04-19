using System;
using Vodovoz.Domain;
using Gamma.GtkWidgets;
using System.Collections.Generic;
using Gtk;
using Vodovoz.Domain.Logistic;
using System.Linq;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListDiscrepancyView : Gtk.Bin
	{
		public RouteListDiscrepancyView()
		{
			this.Build();
			Configure();
			Items = new List<Discrepancy>();
		}

		List<Discrepancy> items;
		public List<Discrepancy> Items{ 
			get{
				return items;
			}
			set{
				items = value;
				ytreeview2.ItemsDataSource = items;
			}
		}


		protected void Configure()
		{
			var colorRed = new Gdk.Color(0xee, 0x66, 0x66);
			var colorWhite = new Gdk.Color(0xff, 0xff, 0xff);
			ytreeview2.ColumnsConfig = ColumnsConfigFactory.Create<Discrepancy>()
				.AddColumn("Название")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Выгрузка")
					.AddNumericRenderer(node => node.ToWarehouse)
				.AddColumn("Недовоз")
					.AddTextRenderer(node => node.Returns)
				.AddColumn("От \nклиента")
					.AddNumericRenderer(node=>node.PickedUpFromClient)
				.AddColumn("Расхождения")
					.AddNumericRenderer(node=>node.Remainder)
						.Adjustment(new Gtk.Adjustment(0, 0, 9999, 1, 1, 0))
				.RowCells()					
					.AddSetter<CellRenderer>((cell,node) => cell.CellBackgroundGdk = node.Remainder==0 ? colorWhite : colorRed)
				.Finish();
		}	

		public void FindDiscrepancies(IList<RouteListItem> items, List<ReturnsNode> allReturnsToWarehouse){
			var discrepancies = new List<Discrepancy>();

			var goodsDiscrepancies = GetGoodsDiscrepancies(items, allReturnsToWarehouse);
			var equipmentDiscrepancies = GetEquipmentDiscrepancies(items, allReturnsToWarehouse);

			discrepancies.AddRange(goodsDiscrepancies);
			discrepancies.AddRange(equipmentDiscrepancies);

			Items = discrepancies;
		}

		IList<Discrepancy> GetGoodsDiscrepancies(IList<RouteListItem> items, List<ReturnsNode> allReturnsToWarehouse)
		{
			var discrepancies = new List<Discrepancy>();
			var orderClosingItems = items
				.SelectMany(item => item.Order.OrderItems)
				.Where(item => Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
				.ToList();
			var goodsReturnedFromClient = orderClosingItems.Where(item => !item.Nomenclature.Serial)
				.GroupBy(item => item.Nomenclature,
					item => item.ReturnedCount,
					(nomenclature, amounts) => new
					ReturnsNode
					{
						Name = nomenclature.Name,
						NomenclatureId = nomenclature.Id,
						NomenclatureCategory = nomenclature.Category,
						Amount = amounts.Sum(i => i),
						Trackable=false
					}).ToList();
			var goodsToWarehouse = allReturnsToWarehouse.Where(item => !item.Trackable);
			foreach (var itemFromClient in goodsReturnedFromClient)
			{
				var itemToWarehouse = 
					goodsToWarehouse.FirstOrDefault(item => item.NomenclatureId == itemFromClient.NomenclatureId);
				if (itemToWarehouse == null)
					continue;
				var failedDeliveryGoodsCount = items.Where(item => !item.IsDelivered())
					.SelectMany(item => item.Order.OrderItems)
					.Where(item => Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
					.Where(item => item.Nomenclature.Id == itemFromClient.NomenclatureId)
					.Sum(item => item.ReturnedCount);
				discrepancies.Add(new Discrepancy
					{
						Name = itemFromClient.Name,
						NomenclatureId = itemFromClient.NomenclatureId,
						FromCancelledOrders = failedDeliveryGoodsCount,
						ClientRejected = itemFromClient.Amount-failedDeliveryGoodsCount,
						ToWarehouse = itemToWarehouse.Amount,
						Trackable = false,
					});
			}
			return discrepancies;
		}

		IList<Discrepancy> GetEquipmentDiscrepancies(IList<RouteListItem> items, List<ReturnsNode> allReturnsToWarehouse)
		{
			var discrepancies = new List<Discrepancy>();
			var equipmentRejectedItems = items
				.SelectMany(item => item.Order.OrderEquipments).Where(item => item.Equipment != null)
				.Where(item=>item.Direction==Vodovoz.Domain.Orders.Direction.Deliver)
				.ToList();

			var equipmentPickedUpItems = items
				.SelectMany(item => item.Order.OrderEquipments).Where(item => item.Equipment != null)
				.Where(item => item.Direction == Vodovoz.Domain.Orders.Direction.PickUp)
				.ToList();

			var equipmentRejectedTypes = equipmentRejectedItems
				.GroupBy(
					item => item.Equipment.Nomenclature.Type,
					item => item.Confirmed ? 0 : 1,
					EquipmentTypeGroupingResult.Selector
				).Where(item=>item.Amount>0);

			var equipmentPickedUpTypes = equipmentPickedUpItems
				.GroupBy(
					item => item.Equipment.Nomenclature.Type,
					item => item.Confirmed ? 1 : 0,
					EquipmentTypeGroupingResult.Selector
				).Where(item => item.Amount > 0);

			var equipmentToWarehouseTypes = allReturnsToWarehouse.Where(item => item.Trackable)
				.GroupBy(
					item => item.EquipmentType,
					item => (int)item.Amount,
					EquipmentTypeGroupingResult.Selector
				).Where(item => item.Amount > 0);

			foreach (var fromClient in equipmentRejectedTypes)
			{
				var toWarehouse = equipmentToWarehouseTypes
					.FirstOrDefault(item => item.EquipmentType.Id == fromClient.EquipmentType.Id);
				var pickedUp = equipmentPickedUpTypes
					.FirstOrDefault(item => item.EquipmentType.Id == fromClient.EquipmentType.Id);
				discrepancies.Add(new Discrepancy
					{
						Name=fromClient.EquipmentType.Name,
						ClientRejected = fromClient.Amount,
						ToWarehouse = toWarehouse!=null ? toWarehouse.Amount : 0,
						PickedUpFromClient = pickedUp!=null ? pickedUp.Amount : 0
					});
			}

			foreach (var toWarehouse in equipmentToWarehouseTypes)
			{
				var fromClient = equipmentRejectedTypes
					.FirstOrDefault(item => item.EquipmentType.Id == toWarehouse.EquipmentType.Id);
				var pickedUp = equipmentPickedUpTypes
					.FirstOrDefault(item => item.EquipmentType.Id == toWarehouse.EquipmentType.Id);
				if (fromClient == null)
				{
					discrepancies.Add(new Discrepancy
						{
							Name=toWarehouse.EquipmentType.Name,
							ClientRejected = 0,
							ToWarehouse = toWarehouse.Amount,
							PickedUpFromClient = pickedUp!=null ? pickedUp.Amount : 0
						});
				}
			}

			foreach (var pickedUp in equipmentPickedUpTypes)
			{
				var fromClient = equipmentRejectedTypes
					.FirstOrDefault(item => item.EquipmentType.Id == pickedUp.EquipmentType.Id);
				var toWarehouse = equipmentToWarehouseTypes
					.FirstOrDefault(item => item.EquipmentType.Id == pickedUp.EquipmentType.Id);
				if (fromClient == null && toWarehouse == null)
				{
					discrepancies.Add(new Discrepancy
						{
							Name = pickedUp.EquipmentType.Name,
							ClientRejected = 0,
							ToWarehouse = 0,
							PickedUpFromClient = pickedUp.Amount
						});
				}
			}
			return discrepancies;
		}
	}

	public class EquipmentTypeGroupingResult
	{
		public EquipmentType EquipmentType{get;set;}
		public int Amount{get;set;}
		public static EquipmentTypeGroupingResult Selector(EquipmentType type, IEnumerable<int> amounts)
		{
			return new EquipmentTypeGroupingResult
			{
				EquipmentType = type,
				Amount = amounts.Sum()
			};
		}
	}

	public class Discrepancy
	{
		public string Name{get;set;}
		public int NomenclatureId{get;set;}
		public int Id{get;set;}
		public decimal PickedUpFromClient{ get; set; }
		public decimal ClientRejected{ get; set; }
		public decimal ToWarehouse{ get; set;}
		public decimal FromCancelledOrders{ get; set;}
		public decimal Remainder{
			get{
				return ToWarehouse - ClientRejected - FromCancelledOrders - PickedUpFromClient;
			}
		}
		public string Returns{
			get{
				return ClientRejected > 0 
					? String.Format("{0}({1:+0;-0})", FromCancelledOrders, ClientRejected) 
						: String.Format("{0}", FromCancelledOrders);
			}
		}
		public bool Trackable{ get; set; }
		public NomenclatureCategory NomenclatureCategory{ get; set; }
		public string Serial{ get { 
				if (Trackable) {
					return Id > 0 ? Id.ToString () : "(не определен)";
				} else
					return String.Empty;
			}
		}
	}

}

