using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz.Views.Client
{
	public partial class EdoOperatorView : TabViewBase<EdoOperatorViewModel>
	{
		public EdoOperatorView(EdoOperatorViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			yentryBrandName.Binding.AddBinding(ViewModel.Entity, e => e.BrandName, w => w.Text).InitializeFromSource();
			yentryCode.Binding.AddBinding(ViewModel.Entity, e => e.Code, w => w.Text).InitializeFromSource();

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
		}

	}
}
