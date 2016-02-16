using System;
using System.Collections.Generic;
using Gamma.GtkWidgets;
using QSTDI;
using Vodovoz.Domain.Logistic;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderReturnsView : TdiTabBase
	{
		List<OrderItemReturnsNode> equipmentFromClient;
		List<OrderItemReturnsNode> itemsToClient;

		RouteListItem routeListItem;

		public OrderReturnsView(RouteListItem routeListItem)
		{
			this.routeListItem = routeListItem;	

			
			this.TabName = "Недовоз заказа №" + routeListItem.Order.Id;
			this.Build();
			Configure();
			itemsToClient = new List<OrderItemReturnsNode>();
			var nomenclatures = routeListItem.Order.OrderItems
				.Where(item => Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
				.Where(item => !item.Nomenclature.Serial).ToList();
			foreach(var item in nomenclatures)
			{
				itemsToClient.Add(new OrderItemReturnsNode(item));
			}
			var equipments = routeListItem.Order.OrderEquipments
				.Where(item => item.Direction == Vodovoz.Domain.Orders.Direction.Deliver);
			foreach(var item in equipments)
			{
				var newOrderEquipmentNode = new OrderItemReturnsNode(item);
				itemsToClient.Add(newOrderEquipmentNode);
			}

			equipmentFromClient = new List<OrderItemReturnsNode>();
			var fromClient = routeListItem.Order.OrderEquipments
				.Where(equipment => equipment.Direction == Vodovoz.Domain.Orders.Direction.PickUp).ToList();
			foreach (var item in fromClient)
			{
				var newOrderEquipmentNode = new OrderItemReturnsNode(item);
				equipmentFromClient.Add(newOrderEquipmentNode);
			}
				
			ytreeToClient.ItemsDataSource = itemsToClient;
			ytreeFromClient.ItemsDataSource = equipmentFromClient;
		}

		protected void Configure()
		{
			ytreeToClient.ColumnsConfig = ColumnsConfigFactory.Create<OrderItemReturnsNode>()
				.AddColumn("Название")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Кол-во")
					.AddNumericRenderer(node => node.Count)
						.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? String.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Кол-во по факту:")
					.AddToggleRenderer(node => node.IsDelivered, false)
						.AddSetter((cell, node) => cell.Visible = node.Nomenclature.Serial || node.Nomenclature.Category == Vodovoz.Domain.NomenclatureCategory.rent)
					.AddNumericRenderer(node => node.ActualCount, false)
						.Adjustment(new Gtk.Adjustment(0, 0, 9999, 1, 1, 0))
						.AddSetter((cell,node)=>cell.Adjustment= new Gtk.Adjustment(0,0,node.Count,1,1,0))
						.AddSetter((cell, node) => cell.Editable = !node.IsEquipment)
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? String.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("")
				.Finish();

			ytreeFromClient.ColumnsConfig = ColumnsConfigFactory.Create<OrderItemReturnsNode>()
				.AddColumn("Название")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Забрано у клиента")
					.AddToggleRenderer(node => node.IsDelivered)
				.AddColumn("")
				.Finish();
		}			
	}

	public class OrderItemReturnsNode{
		OrderItem orderItem;
		OrderEquipment orderEquipment;

		public OrderItemReturnsNode(OrderItem item){			
			orderItem = item;
		}

		public OrderItemReturnsNode(OrderEquipment equipment){			
			orderEquipment = equipment;
		}

		public bool IsEquipment{ 
			get{
				return orderEquipment != null;
			}
		}

		public bool IsDelivered{
			get{
				return ActualCount > 0;
			}
			set{				
				ActualCount = value ? 1 : 0;
			}
		}
		public int ActualCount{
			get{
				if (IsEquipment)
				{
					return orderEquipment.Confirmed ? 1 : 0;
				}
				else
				{
					return orderItem.ActualCount;
				}
			}
			set{
				if (!IsEquipment)
					orderItem.ActualCount = value;
				else
					orderEquipment.Confirmed = value > 0;
			}
		}
		public Nomenclature Nomenclature{
			get{
				if (IsEquipment)
				{
					return orderEquipment.Equipment != null ? orderEquipment.Equipment.Nomenclature : orderEquipment.NewEquipmentNomenclature;
				}
				else
				{
					return orderItem.Nomenclature;
				}
			}
		}
		public int Count{ 
			get{
				return IsEquipment ? 1 : orderItem.Count;
			}
		}

		public string Name{
			get{
				return IsEquipment ? orderEquipment.NameString : orderItem.NomenclatureString;
			}
		}			
	}
}

