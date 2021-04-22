using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModels.Warehouses
{
    public class WarehouseViewModel : EntityTabViewModelBase<Warehouse>
    {
        public WarehouseViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
            : base(uowBuilder, unitOfWorkFactory, commonServices)
        {
            TabName = Entity?.Id == 0 ? "Новый склад" : Entity?.Name;
        }
    }
}
