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
			}
			var p_label = new Gtk.Label("+ Новый контргагент");
			var p_widget = new Button() { Name = "New" };
			WidgetPlace.AppendPage(p_widget, p_label);

			var ex_label = new Gtk.Label("Существующий контрагент");
			var ex_widget = new Button() { Name = "Exi" };
			WidgetPlace.AppendPage(ex_widget, ex_label);

			WidgetPlace.ShowAll();


			WidgetPlace.SwitchPage += SwitchPage_WidgetPlace;
			ViewModel.CounterpartyOrdersModelsUpdateEvent += Update_WidgetPlace;

			CostAndDeliveryIntervalButton.Sensitive =
				ViewModel.currentCounterparty.DeliveryPoints != null ||
				ViewModel.currentCounterparty.DeliveryPoints.Count > 0 ? true : false;
		}

		#region Events

		uint lastPage;

		private void SwitchPage_WidgetPlace(object sender, SwitchPageArgs args)
		{
			Notebook place = (Notebook)sender;
			Console.WriteLine($"Текущая страница: {args.PageNum} , {place.GetTabLabelText(place.CurrentPageWidget)}");
			if(args.PageNum == place.NPages - 1 && place.GetTabLabelText(place.CurrentPageWidget) == "Существующий контрагент") {
				ViewModel.ExistingClientCommand();
				place.Page = Convert.ToInt32(lastPage);
			}
			else if(args.PageNum == place.NPages - 2 && place.GetTabLabelText(place.CurrentPageWidget) == "+ Новый контргагент") {
				ViewModel.NewClientCommand();
				place.Page = Convert.ToInt32(lastPage);

			} else {
				Counterparty counterparty = (place.CurrentPageWidget as CounterpartyOrderView).ViewModel.Client;
				ViewModel.UpadateCurrentCounterparty(counterparty);
				lastPage = args.PageNum;
			}
		}

		public void Update_WidgetPlace()
		{
			int count = WidgetPlace.NPages;
			for(int i = 0; i < count; i++) {
				WidgetPlace.RemovePage(0);
			}
			foreach(var item in ViewModel.CounterpartyOrdersModels) {
				var label = new Gtk.Label(item.Client.Name);
				var widget = new CounterpartyOrderView(item);
				WidgetPlace.AppendPage(widget, label);
			}
			var p_label = new Gtk.Label("+ Новый контргагент");
			var p_widget = new Button() { Name = "New" };
			WidgetPlace.AppendPage(p_widget, p_label);

			var ex_label = new Gtk.Label("Существующий контрагент");
			var ex_widget = new Button() { Name = "Exit" };
			WidgetPlace.AppendPage(ex_widget, ex_label);

			WidgetPlace.ShowAll();

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
