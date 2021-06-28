using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Store;

namespace Vodovoz.Views.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MovementWagonView : TabViewBase<MovementWagonViewModel>
	{
		public MovementWagonView(MovementWagonViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ConfigureDlg();
		}

		void ConfigureDlg()
        {
			yentryMovementWagonName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
		}
	}
}
