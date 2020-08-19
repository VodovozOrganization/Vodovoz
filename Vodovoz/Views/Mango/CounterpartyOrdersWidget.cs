using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Client;
using Gamma.GtkWidgets;

namespace Vodovoz.Views.Mango
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CounterpartyOrdersWidget : Gtk.Bin
	{
		private string ClientName { get; }
		private List<Order> LatestOrder { get; set; }
		public CounterpartyOrdersWidget(string client, IEnumerable<Order> latestOrder)
		{
			ClientName = client;
			LatestOrder = LatestOrder.ToList<Order>();
			this.Build();
			Refresh();
		}
		void Configure()
		{
			//ContrAgentYEntry.Binding.AddBi3nding()
			//OrderYTreeView.RowAc4tivated += 
			OrderYTreeView.ColumnsConfig = ColumnsConfigFactory.Create<Order>()
			.AddColumn("Номер")
			.AddNumericRenderer(order => order.Id)
			.AddColumn("Дата")
			.AddTextRenderer(order => order.DeliveryDate.HasValue ? order.DeliveryDate.Value.ToString("dd.MM.yy") : String.Empty)
			.AddColumn("Статус")
			.AddTextRenderer(order => order.OrderStatus.ToString())
			.AddColumn("Адрес")
			.AddTextRenderer(order => order.DeliveryPoint.CompiledAddress)
			.Finish();
		}
		void Refresh()
		{
			OrderYTreeView.SetItemsSource<Order>(LatestOrder);
			LatestOrdersBo.Visible = LatestOrder.Any<Order>();
		}
	}
}
