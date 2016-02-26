using System;
using System.Collections.Generic;
using Gamma.GtkWidgets;
using QSTDI;
using Vodovoz.Domain.Logistic;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;
using System.Data.Bindings.Collections.Generic;
using System.ComponentModel;
using QSProjectsLib;

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
			ytreeToClient.Sensitive = routeListItem.IsDelivered();
			ytreeFromClient.Sensitive = routeListItem.IsDelivered();
			Configure();
			itemsToClient = new List<OrderItemReturnsNode>();
			var nomenclatures = routeListItem.Order.OrderItems
				.Where(item => Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
				.Where(item => !item.Nomenclature.Serial).ToList();
			foreach(var item in nomenclatures)
			{
				itemsToClient.Add(new OrderItemReturnsNode(item));
				item.PropertyChanged += OnOrderChanged;
			}
			var equipments = routeListItem.Order.OrderEquipments
				.Where(item => item.Direction == Vodovoz.Domain.Orders.Direction.Deliver);
			foreach(var item in equipments)
			{				
				itemsToClient.Add(new OrderItemReturnsNode(item));
				item.PropertyChanged += OnOrderChanged;
			}

			equipmentFromClient = new List<OrderItemReturnsNode>();
			var fromClient = routeListItem.Order.OrderEquipments
				.Where(equipment => equipment.Direction == Vodovoz.Domain.Orders.Direction.PickUp).ToList();
			foreach (var item in fromClient)
			{
				var newOrderEquipmentNode = new OrderItemReturnsNode(item);
				equipmentFromClient.Add(newOrderEquipmentNode);
			}
			entryTotal.Text = CurrencyWorks.GetShortCurrencyString(routeListItem.Order.ActualGoodsTotalSum);

			ytreeToClient.ItemsDataSource = itemsToClient;
			ytreeFromClient.ItemsDataSource = equipmentFromClient;
		}

		public void OnOrderChanged(object sender, PropertyChangedEventArgs args)
		{
			entryTotal.Text = CurrencyWorks.GetShortCurrencyString(routeListItem.Order.ActualGoodsTotalSum);
		}

		protected void Configure()
		{
			yentryCounterparty.SubjectType = typeof(Counterparty);
			yentryCounterparty.Binding.AddBinding(routeListItem.Order, o => o.Client, w => w.Subject).InitializeFromSource();
			yentryCounterparty.CanEditReference = false;

			yentryDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			yentryDeliveryPoint.Binding.AddBinding(routeListItem.Order, o => o.DeliveryPoint, w => w.Subject).InitializeFromSource();
			yentryDeliveryPoint.CanEditReference = false;

			ytreeToClient.ColumnsConfig = ColumnsConfigFactory.Create<OrderItemReturnsNode>()
				.AddColumn("Название")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Кол-во")
					.AddNumericRenderer(node => node.Count)
						.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? String.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Кол-во по факту")
					.AddToggleRenderer(node => node.IsDelivered, false)
						.AddSetter((cell, node) => cell.Visible = node.Nomenclature.Serial || node.Nomenclature.Category == Vodovoz.Domain.NomenclatureCategory.rent)
					.AddNumericRenderer(node => node.ActualCount, false)
						.Adjustment(new Gtk.Adjustment(0, 0, 9999, 1, 1, 0))
						.AddSetter((cell,node)=>cell.Adjustment= new Gtk.Adjustment(0,0,node.Count,1,1,0))
						.AddSetter((cell, node) => cell.Editable = !node.IsEquipment)
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? String.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Цена")
					.AddNumericRenderer(node=>node.Price)
						.Adjustment(new Gtk.Adjustment(0,0,99999,1,100,0))
						.AddSetter((cell,node)=>cell.Editable = node.HasPrice)
					.AddTextRenderer (node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("Стоимость")
					.AddNumericRenderer(node=>node.Sum)
					.AddTextRenderer(node=>CurrencyWorks.CurrencyShortName)
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

		public bool HasPrice{
			get{
				return !IsEquipment || orderEquipment.OrderItem != null;
			}
		}

		public decimal Price{
			get{
				if (IsEquipment)
				{
					return orderEquipment.OrderItem != null ? orderEquipment.OrderItem.Price : 0;
				}
				else
					return orderItem.Price;
			}
			set{
				if (IsEquipment)
				{
					if (orderEquipment.OrderItem != null)
						orderEquipment.OrderItem.Price = value;					
				}
				else
					orderItem.Price = value;
			}
		}
		public decimal Sum{
			get{ 
				return Price * ActualCount; 
			}
		}

	}
}

