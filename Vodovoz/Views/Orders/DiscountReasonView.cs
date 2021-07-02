using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class DiscountReasonView : TabViewBase<DiscountReasonViewModel>
	{
		public DiscountReasonView(DiscountReasonViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			entryName.Binding.AddBinding(ViewModel.Entity, dr => dr.Name, w => w.Text).InitializeFromSource();
			checkIsArchive.Binding.AddBinding(ViewModel.Entity, dr => dr.IsArchive, w => w.Active).InitializeFromSource();
			
			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
		}
	}
}
