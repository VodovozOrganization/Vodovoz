using System;
using System.Linq;
using Gtk;
using QS.Views.Dialog;
using Vodovoz.Domain.Client;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.ViewModels.Mango;
using Vodovoz.Domain.Orders;
using Gamma.GtkWidgets;
using Gtk;
using Vodovoz.Domain.Client;
using FluentNHibernate.Data;
using System.Collections.Generic;

using Vodovoz.Views.Mango.Incoming;

namespace Vodovoz.Views.Mango
{
	public partial class IncomingCallView : DialogViewBase<IncomingCallViewModel>
	{
		public IncomingCallView(IncomingCallViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();

		}
		void Configure() {

			ViewModel.MangoManager.PropertyChanged += MangoManager_PropertyChanged;
			RefreshIncomings();
			if(ViewModel.CounterpartyOrdersModels != null) {
				WidgetPlace.Visible = true;
				foreach(var item in ViewModel.CounterpartyOrdersModels) {
					var label = new Gtk.Label(item.Client.Name);
					var widget = new CounterpartyOrderView(item);
					WidgetPlace.AppendPage(widget, label);
					WidgetPlace.ShowAll();
				}
			} else {
				WidgetPlace.Visible = false;
			}
			//WidgetPlace.ChangeCurrentPage += ChangeCurrentPage_WidgetPlace;
			//ViewModel.CounterpartyOrdersModelsUpdateEvent += Update_WidgetPlace;
		}

		void MangoManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(MangoManager.IncomingCalls)) {
				RefreshIncomings();
			}

			if(e.PropertyName == "IncomingCalls.Time") {
				RefreshTimes();
			}
		}

		private void ChangeCurrentPage_WidgetPlace(object sender, EventArgs e)
		{
			Notebook widget = (Notebook)sender;
			Counterparty counterparty = (widget.CurrentPageWidget as CounterpartyOrderView).ViewModel.Client;
			//ViewModel.UpadateCurrentCounterparty(counterparty);
		}

		protected void OnButtonDisconnectClicked(object sender, EventArgs e)
		{
			ViewModel.DeclineCall();
		}

		#region Private

		private void RefreshIncomings()
		{
			var count = vboxIncomings.Children.Length;

			int i = 0;
			foreach(var incoming in ViewModel.MangoManager.IncomingCalls) {
				if(i >= count) {
					vboxIncomings.Add(new IncomingView());
					vboxIncomings.ShowAll();
				}
				var view = (IncomingView)vboxIncomings.Children[i];
				view.Number = incoming.CallerNumber;
				view.Time = incoming.StageDuration.Value;
				view.CallerName = incoming.CallerName;
				i++;
				if(i > 10)
					break;
			}

			foreach(var view in vboxIncomings.Children.Skip(i).Reverse())
				vboxIncomings.Remove(view);
		}

		private void RefreshTimes()
		{
			int i = 0;
			foreach(IncomingView view in vboxIncomings.Children) {
				view.Time = ViewModel.MangoManager.IncomingCalls[i].StageDuration.Value;
				i++;
			}
		}

		#endregion
	}
}
