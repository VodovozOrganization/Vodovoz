using System;
using System.Linq;
using Gtk;
using QS.Views.Dialog;
using Vodovoz.Domain.Client;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.ViewModels.Mango;

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

			OnLinePlace.Visible = ViewModel.ShowTransferCaller;
			LinePhone.Binding.AddBinding(ViewModel, v => v.OnLineText, l => l.LabelProp).InitializeFromSource();

			ViewModel.MangoManager.PropertyChanged += MangoManager_PropertyChanged;
			RefreshIncomings();
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
