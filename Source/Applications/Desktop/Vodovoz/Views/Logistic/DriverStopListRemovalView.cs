using QS.Views.Dialog;
using Vodovoz.ViewModels.Logistic.DriversStopLists;

namespace Vodovoz.Views.Logistic
{
	[WindowSize(400, 600)]
	public partial class DriverStopListRemovalView : DialogViewBase<DriverStopListRemovalViewModel>
	{
		public DriverStopListRemovalView(DriverStopListRemovalViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			if(ViewModel == null)
			{
				return;
			}

			ylabelDriverInfo.Binding
				.AddBinding(ViewModel, vm => vm.DriverInfo, v => v.LabelProp)
				.InitializeFromSource();

			ytextviewComment.Binding
				.AddBinding(ViewModel, vm => vm.DriverStopListRemoval.Comment, v => v.Buffer.Text)
				.InitializeFromSource();

			ybuttonOk.Clicked += (s, e) => ViewModel.CreateCommand?.Execute();
			ybuttonCancel.Clicked += (s, e) => ViewModel.CancelCommand?.Execute();

			radiobutton1Hour.Toggled += (s, e) => ViewModel.SetSelectedPeriod1HourCommand?.Execute();
			radiobutton3Hours.Toggled += (s, e) => ViewModel.SetSelectedPeriod3HourCommand?.Execute();
			radiobutton24Hours.Toggled += (s, e) => ViewModel.SetSelectedPeriod24HoursCommand?.Execute();
		}
	}
}
