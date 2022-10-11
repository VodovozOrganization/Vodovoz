using QS.Views.GtkUI;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class LateArrivalReasonView : TabViewBase<LateArrivalReasonViewModel>
	{
		public LateArrivalReasonView(LateArrivalReasonViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			ybtnSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			ybtnCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);

			yentryLateArrivalReasonName.Binding.AddBinding(ViewModel.Entity, vm => vm.Name, w => w.Text).InitializeFromSource();
			yChkIsArchive.Binding.AddBinding(ViewModel.Entity, vm => vm.IsArchive, w => w.Active).InitializeFromSource();
		}
	}
}
