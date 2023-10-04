using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic.Drivers;
using Autofac;
using QS.ViewModels.Control.EEVM;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class DriverWarehouseEventViewModel : EntityTabViewModelBase<DriverWarehouseEvent>
	{
		private readonly ILifetimeScope _scope;

		public DriverWarehouseEventViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ILifetimeScope scope) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_scope = scope ?? throw new System.ArgumentNullException(nameof(scope));

			ConfigureEntityChangingRelations();
			ConfigureEntryViewModels();
		}

		public bool IdGtZero => Entity.Id > 0;
		public IEntityEntryViewModel DriverWarehouseEventNameViewModel { get; private set; }

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.Id, () => IdGtZero);
		}

		private void ConfigureEntryViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<DriverWarehouseEvent>(this, Entity, UoW, NavigationManager, _scope);

			DriverWarehouseEventNameViewModel = builder.ForProperty(x => x.EventName)
				.UseViewModelJournalAndAutocompleter<DriversWarehousesEventsJournalViewModel>()
				.UseViewModelDialog<DriverWarehouseEventViewModel>()
				.Finish();
		}
	}
}
