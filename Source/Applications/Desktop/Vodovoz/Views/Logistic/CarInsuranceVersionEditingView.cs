using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Widgets.Cars.Insurance;
namespace Vodovoz.Views.Logistic
{
	[ToolboxItem(true)]
	public partial class CarInsuranceVersionEditingView : WidgetViewBase<CarInsuranceVersionEditingViewModel>
	{
		public CarInsuranceVersionEditingView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			yenumcomboboxInsuranceType.Sensitive = false;
			yenumcomboboxInsuranceType.ItemsEnum = typeof(CarInsuranceType);
			yenumcomboboxInsuranceType.Binding
				.AddBinding(ViewModel.Insurance, e => e.InsuranceType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			daterangepickerPeriod.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			yentryInsuranceNumber.Binding
				.AddBinding(ViewModel, vm => vm.InsuranceNumber, w => w.Text)
				.InitializeFromSource();

			ConfigureInsurerEntityEntry();
		}

		private void ConfigureInsurerEntityEntry()
		{
			entityentryInsurer.ViewModel =
				new LegacyEEVMBuilderFactory<CarInsuranceVersionEditingViewModel>((ITdiTab)ViewModel.ParentDialog, ViewModel, ViewModel.UnitOfWork, ViewModel.NavigationManager, ViewModel.LifetimeScope)
				.ForProperty(x => x.Insurer)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>((filter) =>
				{
					filter.CounterpartyType = Domain.Client.CounterpartyType.Supplier;
				})
				.Finish();
		}
	}
}
