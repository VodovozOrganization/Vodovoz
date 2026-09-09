using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Widgets.Cars;
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

			yvboxMain.Sensitive = ViewModel.CanEditAdditionalFuelTypes;

			entityentryFuelType.ViewModel = ViewModel.FuelTypeViewModel;

			ytreeviewFuelTypes.ColumnsConfig = FluentColumnsConfig<FuelType>.Create()
				.AddColumn("Вид топлива")
					.AddTextRenderer(x => x.Name).XAlign(0.5f)
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
