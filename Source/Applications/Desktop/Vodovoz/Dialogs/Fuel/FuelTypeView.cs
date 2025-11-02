using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Core.Domain.Fuel;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Dialogs.Fuel;

namespace Vodovoz.Dialogs.Fuel
{
	[ToolboxItem(true)]
	public partial class FuelTypeView : TabViewBase<FuelTypeViewModel>
	{
		public FuelTypeView(FuelTypeViewModel fuelTypeViewModel) : base(fuelTypeViewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryName.Sensitive = yspinbuttonCost.Sensitive = buttonSave.Sensitive = ViewModel.CanEdit;
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			yspinbuttonCost.Binding.AddBinding(ViewModel, e => e.FuelPrice, w => w.ValueAsDecimal).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(ViewModel.AskSaveOnClose, QS.Navigation.CloseSource.Cancel); };

			datepickerVersionDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SelectedDate, w => w.DateOrNull)
				.InitializeFromSource();

			ytreeFuelPriceVersions.ColumnsConfig = FluentColumnsConfig<FuelPriceVersion>.Create()
				.AddColumn("Код").MinWidth(50).HeaderAlignment(0.5f).AddTextRenderer(x => x.Id == 0 ? "Новая" : x.Id.ToString()).XAlign(0.5f)
				.AddColumn("Цена").AddTextRenderer(x => x.FuelPrice.ToString()).XAlign(0.5f)
				.AddColumn("Начало действия").AddTextRenderer(x => x.StartDate.ToString("g")).XAlign(0.5f)
				.AddColumn("Окончание действия").AddTextRenderer(x => x.EndDate.HasValue ? x.EndDate.Value.ToString("g") : "").XAlign(0.5f)
				.AddColumn("")
				.Finish();
			ytreeFuelPriceVersions.ItemsDataSource = ViewModel.Entity.ObservableFuelPriceVersions;
			ytreeFuelPriceVersions.Binding.AddBinding(ViewModel, vm => vm.SelectedFuelPriceVersion, w => w.SelectedRow).InitializeFromSource();

			ytreeviewGazpromProductGroups.ColumnsConfig = FluentColumnsConfig<GazpromFuelProductsGroup>.Create()
				.AddColumn("Наименование").HeaderAlignment(0.5f).AddTextRenderer(x => x.GazpromFuelProductGroupName).XAlign(0.5f)
				.AddColumn("Код").MinWidth(120).HeaderAlignment(0.5f).AddTextRenderer(x => x.GazpromFuelProductGroupId).XAlign(0.5f)
				.AddColumn("")
				.Finish();
			ytreeviewGazpromProductGroups.ItemsDataSource = ViewModel.FuelProductGroups;

			ytreeviewProductsInGroup.ColumnsConfig = FluentColumnsConfig<GazpromFuelProduct>.Create()
				.AddColumn("Наименование").HeaderAlignment(0.5f).AddTextRenderer(x => x.GazpromFuelProductName).XAlign(0.5f)
				.AddColumn("Код").MinWidth(120).HeaderAlignment(0.5f).AddTextRenderer(x => x.GazpromFuelProductId).XAlign(0.5f)
				.AddColumn("")
				.Finish();
			ytreeviewProductsInGroup.ItemsDataSource = ViewModel.FuelProductsInGroup;

			buttonNewVersion.Binding.AddBinding(ViewModel, vm => vm.CanAddNewFuelVersion, w => w.Sensitive).InitializeFromSource();
			buttonNewVersion.Clicked += (sender, args) => ViewModel.AddNewCarFuelVersion();

			buttonChangeVersionDate.Binding.AddBinding(ViewModel, vm => vm.CanChangeFuelVersionDate, w => w.Sensitive).InitializeFromSource();
			buttonChangeVersionDate.Clicked += (sender, args) => ViewModel.ChangeFuelVersionStartDate();
		}
	}
}
