using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz.Views.Client
{
	public partial class UnsubscribingReasonView : TabViewBase<UnsubscribingReasonViewModel>
	{
		public UnsubscribingReasonView(UnsubscribingReasonViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			ycheckbuttonIsArchive.Binding.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();
			ycheckbuttonIsOtherReason.Binding.AddBinding(ViewModel.Entity, e => e.IsOtherReason, w => w.Active).InitializeFromSource();
			
			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
		}
	}
}
