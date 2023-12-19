using System;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.RentPackages;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Rent
{
    public class PaidRentPackageViewModel : EntityTabViewModelBase<PaidRentPackage>
    {
		private readonly INomenclatureJournalFactory _nomenclatureJournalFactory;
		private readonly IRentPackageRepository _rentPackageRepository;
		private ILifetimeScope _lifetimeScope;
		private IEntityAutocompleteSelectorFactory _depositServiceSelectorFactory;
		private IEntityAutocompleteSelectorFactory _nomenclatureServiceSelectorFactory;

		public PaidRentPackageViewModel(
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

	        NomenclatureCriteria = UoW.Session.CreateCriteria<Nomenclature>();
	        DepositNomenclatureCriteria = NomenclatureCriteria.Add(Restrictions.Eq("Category", NomenclatureCategory.deposit));

	        ConfigureValidateContext();
        }
        
        public ICriteria DepositNomenclatureCriteria { get; }
        public ICriteria NomenclatureCriteria { get; }

		public IEntityAutocompleteSelectorFactory DepositServiceSelectorFactory =>
			_depositServiceSelectorFactory
			?? (_depositServiceSelectorFactory = _nomenclatureJournalFactory.GetDepositSelectorFactory(_lifetimeScope));

		public IEntityAutocompleteSelectorFactory NomenclatureSelectorFactory =>
			_nomenclatureServiceSelectorFactory ??
			(_nomenclatureServiceSelectorFactory = _nomenclatureJournalFactory.GetServiceSelectorFactory(_lifetimeScope));

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
