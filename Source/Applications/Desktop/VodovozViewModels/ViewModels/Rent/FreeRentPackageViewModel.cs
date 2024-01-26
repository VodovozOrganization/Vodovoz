using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using Vodovoz.Domain;
using Vodovoz.EntityRepositories.RentPackages;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.ViewModels.Rent
{
	public class FreeRentPackageViewModel : EntityTabViewModelBase<FreeRentPackage>
	{
		private readonly IRentPackageRepository _rentPackageRepository;

		public FreeRentPackageViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IRentPackageRepository rentPackageRepository,
			ILifetimeScope lifetimeScope)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(lifetimeScope is null)
			{
				throw new ArgumentNullException(nameof(lifetimeScope));
			}

			_rentPackageRepository = rentPackageRepository ?? throw new ArgumentNullException(nameof(rentPackageRepository));
			
			ConfigureValidateContext();

			DepositServiceNomenclatureViewModel = new CommonEEVMBuilderFactory<FreeRentPackage>(this, Entity, UoW, NavigationManager, lifetimeScope)
				.ForProperty(x => x.DepositService)
				.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel, NomenclatureFilterViewModel>(filter =>
				{

				})
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();
		}

		public IEntityEntryViewModel DepositServiceNomenclatureViewModel { get; }

		private void ConfigureValidateContext()
		{
			ValidationContext.ServiceContainer.AddService(typeof(IRentPackageRepository), _rentPackageRepository);
		}
	}
}
