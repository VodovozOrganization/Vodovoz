using System.Linq;
using Gtk;
using Vodovoz.ViewModels.Dialogs.Fuel;

namespace Vodovoz.Dialogs.Fuel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FuelBalanceView : Gtk.Bin
	{
		private FuelBalanceViewModel viewModel;
		private VBox allBalanceVBox;
		private VBox subdivisionsBalanceVBox;

		public FuelBalanceViewModel ViewModel {
			get { return viewModel; }
			set {
				viewModel = value;
				ConfigureDlg();
			}
		}

		public FuelBalanceView()
		{
			this.Build();
		}

		private void ConfigureDlg()
		{
			ViewModel.PropertyChanged += (sender, e) => {
				if(e.PropertyName == nameof(ViewModel.AllFuelsBalance)) {
					UpdateAllFuelBalance();
				}
				if(e.PropertyName == nameof(ViewModel.SubdivisionsFuelsBalance)) {
					UpdateSubdivisionFuelBalance();
				}
			};
			UpdateAllFuelBalance();
			UpdateSubdivisionFuelBalance();

			labelAllFuelBalance.Binding.AddBinding(ViewModel, vm => vm.HasAllFuelsBalance, w => w.Visible).InitializeFromSource();
			labelSubdivisionsFuelsBalance.Binding.AddBinding(ViewModel, vm => vm.HasSubdivisionsFuelsBalance, w => w.Visible).InitializeFromSource();
		}

		private void UpdateAllFuelBalance()
		{
			if(allBalanceVBox != null) {
				allBalanceVBox.Destroy();
			}
			allBalanceVBox = new VBox();

			var itemHBox = new HBox();
			var itemVoxLeft = new VBox();
			var itemVoxRight = new VBox();
			foreach(var item in ViewModel.AllFuelsBalance) {
				itemVoxLeft.Add(new Label { LabelProp = $"{item.Key.Name}:", Xalign = 0 });
				itemVoxRight.Add(new Label { LabelProp = $"{item.Value.ToString(" 0.##")} л.", Xalign = 0 });
			}
			itemHBox.Add(itemVoxLeft);
			itemHBox.Add(itemVoxRight);
			allBalanceVBox.Add(itemHBox);

			allBalanceVBox.ShowAll();
			vboxAllBalance.Add(allBalanceVBox);
		}

		private void UpdateSubdivisionFuelBalance()
		{
			if(subdivisionsBalanceVBox != null) {
				subdivisionsBalanceVBox.Destroy();
			}

			subdivisionsBalanceVBox = new VBox();

			foreach(var cashSubdivision in ViewModel.SubdivisionsFuelsBalance) {
				if(!cashSubdivision.Value.Any()) {
					continue;
				}
				subdivisionsBalanceVBox.Add(new Label(""));

				subdivisionsBalanceVBox.Add(new Label { LabelProp = cashSubdivision.Key.Name, Xalign = 0 });
				var itemHBox = new HBox();
				var itemVoxLeft = new VBox();
				var itemVoxRight = new VBox();
				foreach(var item in cashSubdivision.Value) {
					itemVoxLeft.Add(new Label { LabelProp = $"{item.Key.Name}:", Xalign = 0 });
					itemVoxRight.Add(new Label { LabelProp = $"{item.Value.ToString("0.##")} л.", Xalign = 0 });
				}
				itemHBox.Add(itemVoxLeft);
				itemHBox.Add(itemVoxRight);
				subdivisionsBalanceVBox.Add(itemHBox);
			}

			subdivisionsBalanceVBox.ShowAll();
			vboxSubdivisionsBalance.Add(subdivisionsBalanceVBox);
		}
	}
}
