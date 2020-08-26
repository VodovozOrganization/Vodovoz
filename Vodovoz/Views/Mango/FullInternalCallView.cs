using System;
using Vodovoz.Domain.Orders;
using Gamma.GtkWidgets;
using QS.Views.Dialog;
using Gtk;
using Vodovoz.ViewModels.Mango;
using Vodovoz.Domain.Client;
using FluentNHibernate.Data;

namespace Vodovoz.Views.Mango
{
	public partial class FullInternalCallView : DialogViewBase<FullInternalCallViewModel>
	{
		public FullInternalCallView(FullInternalCallViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			CallNumberYLabel.Text = ViewModel.Phone.Number;

			foreach(var item in ViewModel.CounterpartyOrdersModels) {
				var label = new Gtk.Label(item.Client.Name);
				var widget = new CounterpartyOrderView(item);
				WidgetPlace.AppendPage(widget, label);
				WidgetPlace.ShowAll();
			}
			WidgetPlace.ChangeCurrentPage += ChangeCurrentPage_WidgetPlace;

			ComplaintButton.Clicked += Clicked_ComplaintButton;
			BottleButton.Clicked += Clicked_BottleButton;
			StockBalanceButton.Clicked += Clicked_StockBalnceButton;
			CostAndDeliveryIntervalButton.Clicked += Clicked_CostAndDeliveryIntervalButton;
			NewOrderButton.Clicked += Clicked_NewClientButton;
			ExistingClientButton.Clicked += Clicked_ExistingClientButton;
			NewOrderButton.Clicked += Clicked_NewOrderButton;
		}

		#region Events
		private void ChangeCurrentPage_WidgetPlace(object sender, EventArgs e)
		{
			Notebook widget = (Notebook)sender;
			Counterparty counterparty = (widget.CurrentPageWidget as CounterpartyOrderView).ViewModel.Client;
			ViewModel.UpadateCurrentCounterparty(counterparty);
		}

		private void Clicked_ExistingClientButton(object sender, EventArgs e)
		{
			ViewModel.ExistingClientCommand();
		}

		protected void Clicked_NewClientButton(object sender, EventArgs e)
		{
			ViewModel.NewClientCommand();
		}

		private void Clicked_ComplaintButton(object sender, EventArgs e)
		{
			ViewModel.AddComplainCommand();

		}

		private void Clicked_BottleButton(object sender, EventArgs e)
		{
			ViewModel.BottleActCommand();
		}

		private void Clicked_StockBalnceButton(object sender, EventArgs e)
		{
			ViewModel.StockBalanceCommand();
		}

		private void Clicked_CostAndDeliveryIntervalButton(object sender, EventArgs e)
		{
			ViewModel.CostAndDeliveryIntervalCommand();
		}
		
		private void Clicked_NewOrderButton(object sender, EventArgs e)
		{
			ViewModel.NewOrderCommand();
		}


		#endregion
	}
}
