using System;
using System.Collections.Generic;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository;

namespace Vodovoz.Panel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CounterpartyPanelView : Gtk.Bin, IPanelView
	{		
		Counterparty Counterparty{get;set;}

		public CounterpartyPanelView()
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			labelName.LineWrapMode = Pango.WrapMode.WordChar;
			labelLatestOrderDate.LineWrapMode = Pango.WrapMode.WordChar;
			ytreeCurrentOrders.ColumnsConfig = ColumnsConfigFactory.Create<Order>()
				.AddColumn("Номер")
					.AddNumericRenderer(node => node.Id)
				.AddColumn("Дата")
					.AddTextRenderer(node => node.DeliveryDate.ToShortDateString())
				.AddColumn("Статус")
					.AddTextRenderer(node => node.OrderStatus.GetEnumTitle())
				.Finish();			
		}

		#region IPanelView implementation
		public IInfoProvider InfoProvider{ get; set; }

		public void Refresh()
		{
			Counterparty = (InfoProvider as ICounterpartyInfoProvider)?.Counterparty;
			if (Counterparty == null)
				return;
			labelName.Text = Counterparty.FullName;
			textviewComment.Buffer.Text = Counterparty.Comment;
			var debt = MoneyRepository.GetCounterpartyDebt(InfoProvider.UoW, Counterparty);
			labelDebt.Text = CurrencyWorks.GetShortCurrencyString(debt);
			var latestOrder = OrderRepository.GetLatestCompleteOrderForCounterparty(InfoProvider.UoW, Counterparty);
			if (latestOrder != null)
			{
				var daysFromLastOrder = (DateTime.Today - latestOrder.DeliveryDate).Days;
				labelLatestOrderDate.Text = String.Format(
					"{0} ({1} {2} назад)",
					latestOrder.DeliveryDate.ToShortDateString(),
					daysFromLastOrder,
					RusNumber.Case(daysFromLastOrder, "день", "дня", "дней")
				);
			}
			else
			{
				labelLatestOrderDate.Text = "(Выполненных заказов нет)";
			}
			var currentOrders = OrderRepository.GetCurrentOrders(InfoProvider.UoW, Counterparty);
			ytreeCurrentOrders.SetItemsSource<Order>(currentOrders);
			vboxCurrentOrders.Visible = currentOrders.Count > 0;
		}

		public bool VisibleOnPanel
		{
			get
			{
				return Counterparty != null;
			}
		}
			
		public void OnCurrentObjectChanged(object changedObject)
		{			
			if (changedObject is Counterparty)
			{
				Refresh();
			}
		}			
		#endregion
	}
}

