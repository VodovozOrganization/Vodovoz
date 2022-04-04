using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Dialogs.Fuel;

namespace Vodovoz.Dialogs.Fuel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FuelTypeView : TabViewBase<FuelTypeViewModel>
	{
		public FuelTypeView(FuelTypeViewModel fuelTypeViewModel) : base(fuelTypeViewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryName.Sensitive = yspinbuttonCost.Sensitive = buttonSave.Sensitive = ViewModel.CanEdit;
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			yspinbuttonCost.Binding.AddBinding(ViewModel.Entity, e => e.Cost, w => w.ValueAsDecimal).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(ViewModel.AskSaveOnClose, QS.Navigation.CloseSource.Cancel); };
		}
	}
}
