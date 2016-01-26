using System;
using System.Collections.Generic;
using Gamma.GtkWidgets;
using QSTDI;
using Vodovoz.Domain.Logistic;
using System.Linq;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderClosingView : TdiTabBase
	{	
		
		List<OrderClosingItem> items;

		RouteListItem routeListItem;

		public OrderClosingView(RouteListClosingItem routeListClosingItem)
		{			
			this.routeListItem = routeListClosingItem.RouteListItem;
			items = routeListClosingItem.OrderClosingItems;
			this.TabName = "Недовоз заказа #" + routeListItem.Order.Id;
			this.Build();
			Configure();
			ytreeview2.ItemsDataSource = items;
		}

		protected void Configure()
		{
			var config = ColumnsConfigFactory.Create<OrderClosingItem>()
				.AddColumn("Название")
					.AddTextRenderer(node => node.OrderItem.NomenclatureString)
				.AddColumn("Кол-во")
					.AddNumericRenderer(node => node.OrderItem.Count)
						.AddSetter((c, node) => c.Digits = node.OrderItem.Nomenclature.Unit == null ? 0 : (uint)node.OrderItem.Nomenclature.Unit.Digits)
					.AddTextRenderer(node => node.OrderItem.Nomenclature.Unit == null ? String.Empty : node.OrderItem.Nomenclature.Unit.Name, false)
				.AddColumn("Недовоз")
					.AddToggleRenderer(node => node.Returned, false)						
						.AddSetter((cell, node) => cell.Visible = node.OrderItem.Nomenclature.Serial || node.OrderItem.Nomenclature.Category==Vodovoz.Domain.NomenclatureCategory.rent)
					.AddNumericRenderer(node => node.Amount, false)
						.Adjustment(new Gtk.Adjustment(0, 0, 9999, 1, 1, 0))
				.AddSetter((cell,node)=>cell.Adjustment= new Gtk.Adjustment(0,0,node.OrderItem.Count,1,1,0))
						.AddSetter((cell, node) => cell.Editable = !node.OrderItem.Nomenclature.Serial && node.OrderItem.Nomenclature.Category!=Vodovoz.Domain.NomenclatureCategory.rent)
				.AddColumn("")
				.Finish();
			ytreeview2.ColumnsConfig = config;

		}
	}
}

