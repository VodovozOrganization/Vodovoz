using System;
using Vodovoz.Domain.Orders;
using Gamma.GtkWidgets;
using QS.Views.Dialog;
using Gtk;
using Vodovoz.ViewModels.Mango;
using Vodovoz.Domain.Client;
using FluentNHibernate.Data;
using System.Collections.Generic;

namespace Vodovoz.Views.Mango
{
	public partial class FullIncomingCallView : DialogViewBase<FullIncomingCallViewModel>
	{
		public FullIncomingCallView(FullIncomingCallViewModel viewModel) : base(viewModel)
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
			ViewModel.CounterpartyOrdersModelsUpdateEvent += Update_WidgetPlace;
			//ComplaintButton.Clicked += Clicked_ComplaintButton;
			//BottleButton.Clicked += Clicked_BottleButton;
			//StockBalanceButton.Clicked += Clicked_StockBalnceButton;
			//CostAndDeliveryIntervalButton.Clicked += Clicked_CostAndDeliveryIntervalButton;
			//NewOrderButton.Clicked += Clicked_NewClientButton;
			//ExistingClientButton.Clicked += Clicked_ExistingClientButton;
			//NewOrderButton.Clicked += Clicked_NewOrderButton;
		}

		#region Events

		private void ChangeCurrentPage_WidgetPlace(object sender, EventArgs e)
		{
			Notebook widget = (Notebook)sender;
			Counterparty counterparty = (widget.CurrentPageWidget as CounterpartyOrderView).ViewModel.Client;
			ViewModel.UpadateCurrentCounterparty(counterparty);
		}

		public void Update_WidgetPlace()
		{
			int count = WidgetPlace.Children.GetLength(0);
			for(int i = 0; i < count; i++) {
				WidgetPlace.RemovePage(i);
			}
			foreach(var item in ViewModel.CounterpartyOrdersModels) {
				var label = new Gtk.Label(item.Client.Name);
				var widget = new CounterpartyOrderView(item);
				WidgetPlace.AppendPage(widget, label);
				WidgetPlace.ShowAll();
			}
		}
		protected void Clicked_NewClientButton(object sender, EventArgs e)
		{
			ViewModel.NewClientCommand();
		}

		private void Clicked_ExistingClientButton(object sender, EventArgs e)
		{

			ViewModel.ExistingClientCommand();
		}

		private void Clicked_NewOrderButton(object sender, EventArgs e)
		{
			ViewModel.NewOrderCommand();
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

		#region MangoEvents
		protected void Clicked_ForwardingButton(object sender, EventArgs e)
		{
			ViewModel.ForwardCallCommand();
		}

		protected void Clicked_ForwardingToConsultationButton(object sender, EventArgs e)
		{
			ViewModel.ForwardToConsultationCommand();
		}

		protected void Clicked_FinishButton(object sender, EventArgs e)
		{
			ViewModel.FinishCallCommand();
		}

		#endregion

		#endregion
	}
}
