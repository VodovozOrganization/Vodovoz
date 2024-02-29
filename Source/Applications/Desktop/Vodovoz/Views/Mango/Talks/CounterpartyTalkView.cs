using Gtk;
using QS.Views.Dialog;
using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Dialogs.Mango.Talks;

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
			CallNumberYLabel.Binding.AddBinding(ViewModel, v => v.PhoneText, w => w.LabelProp).InitializeFromSource();

			foreach(var item in ViewModel.CounterpartyOrdersViewModels) {
				var label = new Gtk.Label(item.Client.Name);
				var widget = new CounterpartyOrderView(item);
				WidgetPlace.AppendPage(widget, label);
			}
			var p_label = new Gtk.Label("+ Новый контрагент");
			var p_widget = new Button() { Name = "New" };
			WidgetPlace.AppendPage(p_widget, p_label);

			var ex_label = new Gtk.Label("Существующий контрагент");
			var ex_widget = new Button() { Name = "Exist" };
			WidgetPlace.AppendPage(ex_widget, ex_label);

			WidgetPlace.ShowAll();


			WidgetPlace.SwitchPage += SwitchPage_WidgetPlace;
			ViewModel.CounterpartyOrdersModelsUpdateEvent += Update_WidgetPlace;

			CostAndDeliveryIntervalButton.Sensitive = ViewModel.currentCounterparty.DeliveryPoints?.Count > 0;
		}

		#region Events

		uint lastPage;
		bool falseSwitching = false;

		private void SwitchPage_WidgetPlace(object sender, SwitchPageArgs args)
		{
			if(falseSwitching)
				return;
			
			Notebook place = (Notebook)sender;

			if (place != null && args != null)
			{
				Console.WriteLine(
					$"Текущая страница: {args.PageNum} , {place.GetTabLabelText(place.CurrentPageWidget)}");
				if (args.PageNum == place.NPages - 1 &&
				    place.GetTabLabelText(place.CurrentPageWidget) == "Существующий контрагент")
				{
					ViewModel.ExistingClientCommand();
					place.Page = Convert.ToInt32(lastPage);
				}
				else if (args.PageNum == place.NPages - 2 &&
				         place.GetTabLabelText(place.CurrentPageWidget) == "+ Новый контрагент")
				{
					ViewModel.NewClientCommand();
					place.Page = Convert.ToInt32(lastPage);
				}
				else
				{
					Counterparty counterparty = (place.CurrentPageWidget as CounterpartyOrderView).ViewModel.Client;
					ViewModel.UpadateCurrentCounterparty(counterparty);
					lastPage = args.PageNum;
				}
			}
		}

		public void Update_WidgetPlace()
		{
			int count = WidgetPlace.NPages;
			falseSwitching = true;//Потому что в процессе удаления срабатывают события переключения вкладок.
			for(int i = 0; i < count; i++) {
				WidgetPlace.RemovePage(0);
			}
			foreach(var item in ViewModel.CounterpartyOrdersViewModels) {
				var label = new Gtk.Label(item.Client.Name);
				var widget = new CounterpartyOrderView(item);
				WidgetPlace.AppendPage(widget, label);
			}
			var p_label = new Gtk.Label("+ Новый контргагент");
			var p_widget = new Button() { Name = "New" };
			WidgetPlace.AppendPage(p_widget, p_label);

			var ex_label = new Gtk.Label("Существующий контрагент");
			var ex_widget = new Button() { Name = "Exist" };
			WidgetPlace.AppendPage(ex_widget, ex_label);

			WidgetPlace.ShowAll();
			WidgetPlace.Page = (int)lastPage;
			falseSwitching = false;

			CostAndDeliveryIntervalButton.Sensitive = ViewModel.currentCounterparty.DeliveryPoints?.Count > 0;
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
				if(ViewModel.currentCounterparty.DeliveryPoints.Count > 1) {

					Gtk.Menu popupMenu = new Menu();
					foreach(var point in ViewModel.currentCounterparty.DeliveryPoints) {
						MenuItem item = new MenuItem($"{point.ShortAddress}");
						item.ButtonPressEvent += delegate (object s, ButtonPressEventArgs _e) { ViewModel.CostAndDeliveryIntervalCommand(point); };
						popupMenu.Add(item);
					}

					popupMenu.ShowAll();
					popupMenu.Popup();
				}
			} else {
				ViewModel.CostAndDeliveryIntervalCommand(ViewModel.currentCounterparty.DeliveryPoints.First());
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
