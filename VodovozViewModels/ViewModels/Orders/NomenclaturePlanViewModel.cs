using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
    public class NomenclaturePlanViewModel : EntityTabViewModelBase<Nomenclature>
    {
        public NomenclaturePlanViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory uowFactory, ICommonServices commonServices)
            : base(uowBuilder, uowFactory, commonServices)
        {
        }
    }
}
