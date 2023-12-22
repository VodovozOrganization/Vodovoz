using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain;
using Vodovoz.EntityRepositories.RentPackages;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Rent
{
	public class FreeRentPackageViewModel : EntityTabViewModelBase<FreeRentPackage>
    {
		private readonly INomenclatureJournalFactory _nomenclatureJournalFactory;
		private readonly IRentPackageRepository _rentPackageRepository;
		private ILifetimeScope _lifetimeScope;
		private IEntityAutocompleteSelectorFactory _depositServiceSelectorFactory;

		public FreeRentPackageViewModel(
			ILifetimeScope lifetimeScope,
            IEntityUoWBuilder uowBuilder,
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices,
			INomenclatureJournalFactory nomenclatureJournalFactory,
            IRentPackageRepository rentPackageRepository) : base(uowBuilder, unitOfWorkFactory, commonServices)
	    {
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_nomenclatureJournalFactory = nomenclatureJournalFactory ?? throw new ArgumentNullException(nameof(nomenclatureJournalFactory));
			_rentPackageRepository = rentPackageRepository ?? throw new ArgumentNullException(nameof(rentPackageRepository));
		    
		    ConfigureValidateContext();
	    }
        
		public IEntityAutocompleteSelectorFactory DepositServiceSelectorFactory =>
			_depositServiceSelectorFactory
			?? (_depositServiceSelectorFactory = _nomenclatureJournalFactory.GetDepositSelectorFactory(_lifetimeScope));

		private void ConfigureValidateContext()
        {
	        ValidationContext.ServiceContainer.AddService(typeof(IRentPackageRepository), _rentPackageRepository);
        }

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
