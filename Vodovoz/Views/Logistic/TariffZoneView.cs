using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class TariffZoneView : TabViewBase<TariffZoneViewModel>
	{
		public TariffZoneView(TariffZoneViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			entryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			ycheckIsFastDeliveryAvailable.Binding.AddBinding(ViewModel.Entity, e => e.IsFastDeliveryAvailable, w => w.Active).InitializeFromSource();
			entryFrom.Binding.AddBinding(ViewModel.Entity, e => e.FastDeliveryTimeFrom, w => w.Time).InitializeFromSource();
			entryTo.Binding.AddBinding(ViewModel.Entity, e => e.FastDeliveryTimeTo, w => w.Time).InitializeFromSource();

			ybuttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			ybuttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
		}
	}
}
