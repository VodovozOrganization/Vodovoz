using System;
using QS.Views.Dialog;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.ViewModels.Mango;
using Vodovoz.Domain.Orders;
using Gamma.GtkWidgets;
using Gtk;
using Vodovoz.Domain.Client;
using FluentNHibernate.Data;
using System.Collections.Generic;
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
			labelName.Binding.AddBinding(ViewModel.MangoManager, m => m.CallerName, w => w.Text).InitializeFromSource();
			labelNumber.Binding.AddFuncBinding<MangoManager>(ViewModel.MangoManager, m => "Телефон: " + m.CallerNumber, w => w.Text).InitializeFromSource();
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
	}
}
