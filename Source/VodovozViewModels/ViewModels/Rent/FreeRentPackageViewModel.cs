using System;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using Vodovoz.Domain;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.RentPackages;

namespace Vodovoz.ViewModels.ViewModels.Rent
{
    public class FreeRentPackageViewModel : EntityTabViewModelBase<FreeRentPackage>
    {
	    private readonly IRentPackageRepository _rentPackageRepository;

	    public FreeRentPackageViewModel(
            IEntityUoWBuilder uowBuilder,
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices,
            IRentPackageRepository rentPackageRepository) : base(uowBuilder, unitOfWorkFactory, commonServices)
	    {
		    _rentPackageRepository = rentPackageRepository ?? throw new ArgumentNullException(nameof(rentPackageRepository));
		    
		    DepositNomenclatureCriteria = UoW.Session.CreateCriteria<Nomenclature>()
		        .Add(Restrictions.Eq("Category", NomenclatureCategory.deposit));

		    ConfigureValidateContext();
	    }
        
        public ICriteria DepositNomenclatureCriteria { get; }
        
        private void ConfigureValidateContext()
        {
	        ValidationContext.ServiceContainer.AddService(typeof(IRentPackageRepository), _rentPackageRepository);
        }
    }
}
