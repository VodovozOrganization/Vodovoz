using Autofac;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.EntityRepositories.RentPackages;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.ViewModels.Rent
{
	public class PaidRentPackageViewModel : EntityTabViewModelBase<PaidRentPackage>
	{
		private readonly IRentPackageRepository _rentPackageRepository;

		public PaidRentPackageViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IRentPackageRepository rentPackageRepository,
			ILifetimeScope lifetimeScope)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			_rentPackageRepository = rentPackageRepository ?? throw new ArgumentNullException(nameof(rentPackageRepository));

			NomenclatureCriteria = UoW.Session.CreateCriteria<Nomenclature>();
			DepositNomenclatureCriteria = NomenclatureCriteria.Add(Restrictions.Eq("Category", NomenclatureCategory.deposit));

			ConfigureValidateContext();

			DepositServiceNomenclatureViewModel = new CommonEEVMBuilderFactory<PaidRentPackage>(this, Entity, UoW, NavigationManager, lifetimeScope)
				.ForProperty(x => x.DepositService)
				.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel, NomenclatureFilterViewModel>(filter =>
				{
					filter.RestrictCategory = NomenclatureCategory.deposit;
				})
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();

			DailyRentServiceNomenclatureViewModel = new CommonEEVMBuilderFactory<PaidRentPackage>(this, Entity, UoW, NavigationManager, lifetimeScope)
				.ForProperty(x => x.RentServiceDaily)
				.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel, NomenclatureFilterViewModel>(filter =>
				{
					filter.RestrictCategory = NomenclatureCategory.service;
				})
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();


			LongTermRentNomenclatureServiceViewModel = new CommonEEVMBuilderFactory<PaidRentPackage>(this, Entity, UoW, NavigationManager, lifetimeScope)
				.ForProperty(x => x.RentServiceMonthly)
				.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel, NomenclatureFilterViewModel>(filter =>
				{
					filter.RestrictCategory = NomenclatureCategory.service;
				})
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();
		}

		public ICriteria DepositNomenclatureCriteria { get; }
		public ICriteria NomenclatureCriteria { get; }

		public IEntityEntryViewModel DepositServiceNomenclatureViewModel { get; }
		public IEntityEntryViewModel DailyRentServiceNomenclatureViewModel { get; }
		public IEntityEntryViewModel LongTermRentNomenclatureServiceViewModel { get; }

		private void ConfigureValidateContext()
		{
			ValidationContext.ServiceContainer.AddService(typeof(IRentPackageRepository), _rentPackageRepository);
		}
	}
}
