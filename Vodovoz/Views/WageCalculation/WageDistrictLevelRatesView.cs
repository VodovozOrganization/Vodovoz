using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Views.WageCalculation
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WageDistrictLevelRatesView : TabViewBase<WageDistrictLevelRatesViewModel>
	{
		public WageDistrictLevelRatesView(WageDistrictLevelRatesViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			yentryName.Binding.AddBinding(ViewModel.Entity, s => s.Name, w => w.Text).InitializeFromSource();
			chkIsArchive.Binding.AddBinding(ViewModel.Entity, s => s.IsArchive, w => w.Active).InitializeFromSource();
			chkDefaultLevel.Binding.AddBinding(ViewModel.Entity, s => s.IsDefaultLevel, w => w.Active).InitializeFromSource();
			chkDefaultLevelOurCars.Binding.AddBinding(ViewModel.Entity, s => s.IsDefaultLevelForOurCars, w => w.Active).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);

			GenerateTabs();
		}

		Notebook nbDistricts;
		void GenerateTabs()
		{
			if(nbDistricts != null)
				nbDistricts.Destroy();
			nbDistricts = new Notebook();

			foreach(WageDistrictLevelRateViewModel vm in ViewModel.ObservableWageDistrictLevelRateViewModels) {
				var view = new WageDistrictLevelRateView(vm, true);
				VBox vbx = new VBox {
					view
				};
				Box.BoxChild viewBox = (Box.BoxChild)vbx[view];
				viewBox.Fill = true;
				viewBox.Expand = true;
				var scrolledWindow = new ScrolledWindow {
					vbx
				};

				Label tabLabel = new Label {
					UseMarkup = true,
					Markup = vm.Entity.WageDistrict.Name
				};

				nbDistricts.AppendPage(scrolledWindow, tabLabel);
			}

			hbxNotebooksWithDistricts.Add(nbDistricts);
			hbxNotebooksWithDistricts.ShowAll();
		}
	}
}