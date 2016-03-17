using System;
using Vodovoz.Domain;
using QSOrmProject;
using System.ComponentModel;
using Vodovoz.Domain.Operations;
using NHibernate.Transform;
using System.Linq;
using QSProjectsLib;
using Vodovoz.Domain.Orders;
using System.Collections.Generic;
using Gamma.GtkWidgets;
using Vodovoz.Repository;
using Gamma.Utilities;

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
			labelAddress.LineWrapMode = Pango.WrapMode.WordChar;
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
			Console.WriteLine($"CounterpartyPanelView #{this.GetHashCode()} refreshed");
			labelName.Text = Counterparty.FullName;
			labelAddress.Text = Counterparty.Address;
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

