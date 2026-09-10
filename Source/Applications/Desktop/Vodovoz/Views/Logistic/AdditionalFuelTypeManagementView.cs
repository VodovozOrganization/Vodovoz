using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Widgets.Cars;
using VodovozBusiness.Domain.Logistic;
namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AdditionalFuelTypeManagementView : WidgetViewBase<AdditionalFuelTypeManagementViewModel>
	{
		public AdditionalFuelTypeManagementView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			yhboxMain.Sensitive = ViewModel.CanEditAdditionalFuelTypes;

			speciallistcomboboxFuelTypes.Binding
				.AddBinding(ViewModel, vm => vm.SelectedNewFuelType, w => w.SelectedItem)
				.InitializeFromSource();

			speciallistcomboboxFuelTypes.ItemsList = ViewModel.AllFuelTypes;

			ytreeviewFuelTypes.ColumnsConfig = FluentColumnsConfig<AdditionalFuelType>.Create()
				.AddColumn("Вид топлива")
					.AddTextRenderer(x => x.FuelType.Name).XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ytreeviewFuelTypes.Binding
				.AddBinding(ViewModel, vm => vm.SelectedExistingFuelType, w => w.SelectedRow)
				.InitializeFromSource();

			ytreeviewFuelTypes.ItemsDataSource = ViewModel.Entity.AdditionalFuelTypes;

			ybuttonAdd.BindCommand(ViewModel.AddFuelTypeCommand);
			ybuttonRemove.BindCommand(ViewModel.RemoveFuelTypeCommand);
		}
	}
}
