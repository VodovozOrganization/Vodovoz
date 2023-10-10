using System;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.EntityRepositories.RentPackages;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Rent
{
    public class PaidRentPackageViewModel : EntityTabViewModelBase<PaidRentPackage>
    {
		private readonly INomenclatureJournalFactory _nomenclatureJournalFactory;
		private readonly IRentPackageRepository _rentPackageRepository;
		private IEntityAutocompleteSelectorFactory _depositServiceSelectorFactory;
		private IEntityAutocompleteSelectorFactory _nomenclatureServiceSelectorFactory;

		public PaidRentPackageViewModel(
            IEntityUoWBuilder uowBuilder,
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices,
			INomenclatureJournalFactory nomenclatureJournalFactory,
            IRentPackageRepository rentPackageRepository) : base(uowBuilder, unitOfWorkFactory, commonServices)
        {
			_nomenclatureJournalFactory = nomenclatureJournalFactory ?? throw new ArgumentNullException(nameof(nomenclatureJournalFactory));
			_rentPackageRepository = rentPackageRepository ?? throw new ArgumentNullException(nameof(rentPackageRepository));

	        NomenclatureCriteria = UoW.Session.CreateCriteria<Nomenclature>();
	        DepositNomenclatureCriteria = NomenclatureCriteria.Add(Restrictions.Eq("Category", NomenclatureCategory.deposit));

	        ConfigureValidateContext();
        }
        
        public ICriteria DepositNomenclatureCriteria { get; }
        public ICriteria NomenclatureCriteria { get; }

		public IEntityAutocompleteSelectorFactory DepositServiceSelectorFactory
		{
			get
			{
				if(_depositServiceSelectorFactory == null)
				{
					_depositServiceSelectorFactory = _nomenclatureJournalFactory.GetDepositSelectorFactory();
				}
				return _depositServiceSelectorFactory;
			}
		}

		public IEntityAutocompleteSelectorFactory NomenclatureSelectorFactory
		{
			get
			{
				if(_nomenclatureServiceSelectorFactory == null)
				{
					_nomenclatureServiceSelectorFactory = _nomenclatureJournalFactory.GetServiceSelectorFactory();
				}
				return _nomenclatureServiceSelectorFactory;
			}
		}

		private void ConfigureValidateContext()
        {
	        ValidationContext.ServiceContainer.AddService(typeof(IRentPackageRepository), _rentPackageRepository);
        }
    }
}
