using System;
using QS.Views.Dialog;
using Gtk;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Mango.Talks;

namespace Vodovoz.Views.Mango.Talks
{
	public partial class CounterpartyTalkView : DialogViewBase<CounterpartyTalkViewModel>
	{
		public CounterpartyTalkView(CounterpartyTalkViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			CallNumberYLabel.Text = ViewModel.GetPhoneNumber();

			foreach(var item in ViewModel.CounterpartyOrdersModels) {
				var label = new Gtk.Label(item.Client.Name);
				var widget = new CounterpartyOrderView(item);
				WidgetPlace.AppendPage(widget, label);
				WidgetPlace.ShowAll();
			}
			WidgetPlace.SwitchPage += ChangeCurrentPage_WidgetPlace;
			ViewModel.CounterpartyOrdersModelsUpdateEvent += Update_WidgetPlace;
			CostAndDeliveryIntervalButton.Sensitive =
				ViewModel.currentCounterparty.DeliveryPoints != null ||
				ViewModel.currentCounterparty.DeliveryPoints.Count > 0 ? true : false;
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
			CostAndDeliveryIntervalButton.Sensitive =
				ViewModel.currentCounterparty.DeliveryPoints != null ||
				ViewModel.currentCounterparty.DeliveryPoints.Count > 0 ? true : false;
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
			if(ViewModel.currentCounterparty.DeliveryPoints != null || ViewModel.currentCounterparty.DeliveryPoints.Count > 0) {
				Gtk.Menu popupMenu = new Menu();
				foreach(var point in ViewModel.currentCounterparty.DeliveryPoints) {
					MenuItem item = new MenuItem($"{point.ShortAddress}");
					item.ButtonPressEvent += delegate (object s, ButtonPressEventArgs _e) { ViewModel.CostAndDeliveryIntervalCommand(point); };
					popupMenu.Add(item);
				}

				popupMenu.ShowAll();
				popupMenu.Popup();
			}
		}

		#region MangoEvents
		protected void Clicked_ForwardingButton(object sender, EventArgs e)
		{
			ViewModel.ForwardCallCommand();
		}

		protected void Clicked_FinishButton(object sender, EventArgs e)
		{
			ViewModel.FinishCallCommand();
		}

		#endregion

		#endregion
	}
}
