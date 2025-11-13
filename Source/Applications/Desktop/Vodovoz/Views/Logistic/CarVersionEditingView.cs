using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Organizations;
using Vodovoz.ViewModels.Widgets.Cars.CarVersions;
namespace Vodovoz.Views.Logistic
{
	[ToolboxItem(true)]
	public partial class CarVersionEditingView : WidgetViewBase<CarVersionEditingViewModel>
	{
		public CarVersionEditingView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			Visible = false;

			yenumcomboboxCarOwnType.ItemsEnum = typeof(CarOwnType);
			yenumcomboboxCarOwnType.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.SelectedCarOwnType, w => w.SelectedItemOrNull)
				.AddBinding(vm => vm.CanEditCarOwnType, w => w.Sensitive)
				.InitializeFromSource();

			ConfigureCarOwnerEntityEntry();

			entityentryOwner.Binding
				.AddBinding(ViewModel, vm => vm.CanSelectCarOwner, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonSave.Binding
				.AddBinding(ViewModel, vm => vm.CanSaveCarVersion, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonSave.BindCommand(ViewModel.SaveCarVersionCommand);
			ybuttonCancel.BindCommand(ViewModel.CancelEditingCommand);

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void ConfigureCarOwnerEntityEntry()
		{
			entityentryOwner.ViewModel =
				new LegacyEEVMBuilderFactory<CarVersionEditingViewModel>((ITdiTab)ViewModel.ParentDialog, ViewModel, ViewModel.UnitOfWork, ViewModel.NavigationManager, ViewModel.LifetimeScope)
				.ForProperty(x => x.SelectedCarOwner)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.IsWidgetVisible))
			{
				Visible = ViewModel.IsWidgetVisible;
			}

			if(e.PropertyName == nameof(ViewModel.AvailableCarOwnTypes))
			{
				UpdateCarOwnTypeComboboxAvailableItems();
			}
		}

		private void UpdateCarOwnTypeComboboxAvailableItems()
		{
			var hiddenItems = yenumcomboboxCarOwnType.HiddenItems;
			foreach(var item in hiddenItems)
			{
				yenumcomboboxCarOwnType.RemoveEnumFromHideList(item);
			}

			if(ViewModel.AvailableCarOwnTypes is null || !ViewModel.AvailableCarOwnTypes.Contains(CarOwnType.Company))
			{
				yenumcomboboxCarOwnType.AddEnumToHideList(CarOwnType.Company);
			}

			if(ViewModel.AvailableCarOwnTypes is null || !ViewModel.AvailableCarOwnTypes.Contains(CarOwnType.Raskat))
			{
				yenumcomboboxCarOwnType.AddEnumToHideList(CarOwnType.Raskat);
			}

			if(ViewModel.AvailableCarOwnTypes is null || !ViewModel.AvailableCarOwnTypes.Contains(CarOwnType.Driver))
			{
				yenumcomboboxCarOwnType.AddEnumToHideList(CarOwnType.Driver);
			}
		}
	}
}
