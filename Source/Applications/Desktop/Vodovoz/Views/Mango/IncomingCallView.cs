using QS.Views.Dialog;
using System;
using System.Linq;
using Vodovoz.ViewModels.Dialogs.Mango;
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
		}

		void MangoManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(MangoManager.RingingCalls)) {
				RefreshIncomings();
			}

			if(e.PropertyName == "IncomingCalls.Time") {
				RefreshTimes();
			}
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
			foreach(var incoming in ViewModel.MangoManager.RingingCalls) {
				if(i >= count) {
					vboxIncomings.Add(new IncomingView());
					vboxIncomings.ShowAll();
				}
				var view = (IncomingView)vboxIncomings.Children[i];
				view.Number = incoming.CallerNumber;
				view.Time = incoming.StageDuration.Value;
				var callerName = incoming.CallerName;
				if (incoming.Message.IsTransfer)
					callerName += $"\nНа линии: {incoming.OnHoldText}";
				view.CallerName = callerName;
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
			var incomings = ViewModel.MangoManager.RingingCalls.ToArray();
			foreach(IncomingView view in vboxIncomings.Children) {
				if(i < incomings.Length)
				{
					view.Time = incomings[i].StageDuration.Value;
				}

				i++;
			}
		}

		#endregion
	}
}
