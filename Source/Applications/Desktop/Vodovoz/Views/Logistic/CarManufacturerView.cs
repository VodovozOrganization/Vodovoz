using QS.Navigation;
using Vodovoz.ViewModels.ViewModels.Logistic;
using QS.Views.GtkUI;

namespace Vodovoz.Views.Logistic
{
	public partial class CarManufacturerView : TabViewBase<CarManufacturerViewModel>
	{
		public CarManufacturerView(CarManufacturerViewModel viewModel) : base(viewModel)
		{
			Build();

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonSave.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			buttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);

			yentryCarManufacturer.WidthRequest = 400;
			yentryCarManufacturer.MaxLength = 100;
			yentryCarManufacturer.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, e => e.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
