using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Goods;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
    public class NomenclaturePlanViewModel : EntityTabViewModelBase<Nomenclature>
    {
        public NomenclaturePlanViewModel(
	        IEntityUoWBuilder uowBuilder,
	        IUnitOfWorkFactory uowFactory,
	        ICommonServices commonServices,
	        INomenclatureRepository nomenclatureRepository)
            : base(uowBuilder, uowFactory, commonServices)
        {
	        if(nomenclatureRepository == null)
	        {
		        throw new ArgumentNullException(nameof(nomenclatureRepository));
	        }

	        ValidationContext.ServiceContainer.AddService(typeof(INomenclatureRepository), nomenclatureRepository);
        }
    }
}
