using Gamma.Widgets.Additions;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using QS.Widgets;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;

namespace Vodovoz.Filters.GtkViews
{
	public partial class CarFilterView : FilterViewBase<CarJournalFilterViewModel>
	{
		public CarFilterView(CarJournalFilterViewModel carJournalFilterViewModel) : base(carJournalFilterViewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			nullablecheckArchive.RenderMode = RenderMode.Icon;
			nullablecheckArchive.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeIsArchive, w => w.Sensitive)
				.AddBinding(vm => vm.Archive, w => w.Active)
				.InitializeFromSource();

			nullablecheckVisitingMaster.RenderMode = RenderMode.Icon;
			nullablecheckVisitingMaster.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeVisitingMasters, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.VisitingMasters, w => w.Active)
				.InitializeFromSource();

			enumcheckCarTypeOfUse.EnumType = typeof(CarTypeOfUse);
			enumcheckCarTypeOfUse.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeRestrictedCarTypesOfUse, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictedCarTypesOfUse, w => w.SelectedValuesList, new EnumsListConverter<CarTypeOfUse>())
				.InitializeFromSource();

			enumcheckCarOwnType.EnumType = typeof(CarOwnType);
			enumcheckCarOwnType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeRestrictedCarOwnTypes, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictedCarOwnTypes, w => w.SelectedValuesList, new EnumsListConverter<CarOwnType>())
				.InitializeFromSource();

			entryModel.ViewModel = ViewModel.CarModelViewModel;

			entryModel.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeCarModel, w => w.Sensitive)
				.InitializeFromSource();

			ycheckbuttonCarsWithoutOwner.Binding
				.AddBinding(ViewModel, vm => vm.IsOnlyCarsWithoutCarOwner, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonCarsWithoutInsurance.Binding
				.AddBinding(ViewModel, vm => vm.IsOnlyCarsWithoutInsurer, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonUsedInDelivery.Binding
				.AddBinding(ViewModel, vm => vm.IsUsedInDelivery, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonNotUsedInDelivery.Binding
				.AddBinding(ViewModel, vm => vm.IsNotUsedInDelivery, w => w.Active)
				.InitializeFromSource();

			ConfigureInsurerEntityEntry();
			ConfigureCarOwnerEntityEntry();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.ExcludedCarTypesOfUse))
			{
				RefreshHiddenElements();
			}
		}

		private void RefreshHiddenElements()
		{
			enumcheckCarTypeOfUse.ClearEnumHideList();

			if(ViewModel.ExcludedCarTypesOfUse is null || !ViewModel.ExcludedCarTypesOfUse.Any())
			{
				return;
			}

			foreach(var excludedCarTypeOfUse in ViewModel.ExcludedCarTypesOfUse)
			{
				enumcheckCarTypeOfUse.AddEnumToHideList(excludedCarTypeOfUse);
			}
		}

		private void ConfigureInsurerEntityEntry()
		{
			entityentryInsurer.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsOnlyCarsWithoutInsurer, w => w.Sensitive)
				.InitializeFromSource();

			entityentryInsurer.ViewModel =
				new LegacyEEVMBuilderFactory<CarJournalFilterViewModel>(ViewModel.Journal, ViewModel, ViewModel.Journal.UoW, ViewModel.Journal.NavigationManager, ViewModel.Journal.LifetimeScope)
				.ForProperty(x => x.Insurer)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>((filter) =>
				{
					filter.CounterpartyType = CounterpartyType.Supplier;
				})
				.Finish();
		}

		private void ConfigureCarOwnerEntityEntry()
		{
			entityentryCarOwner.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsOnlyCarsWithoutCarOwner, w => w.Sensitive)
				.InitializeFromSource();

			entityentryCarOwner.ViewModel = ViewModel.OrganizationViewModel;
		}

		public override void Destroy()
		{
			ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
			base.Destroy();
		}
	}
}
