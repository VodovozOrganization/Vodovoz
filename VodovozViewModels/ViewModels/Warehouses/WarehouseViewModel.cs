using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Subdivisions;

namespace Vodovoz.ViewModels.Warehouses
{
    public class WarehouseViewModel : EntityTabViewModelBase<Warehouse>
    {
        public WarehouseViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, ISubdivisionRepository subdivisionRepository)
            : base(uowBuilder, unitOfWorkFactory, commonServices)
        {
            TabName = Entity?.Id == 0 ? "Новый склад" : Entity?.Name;
            Subdivisions = subdivisionRepository.GetAllDepartmentsOrderedByName(UoW);
            CanArchiveWarehouse = commonServices.CurrentPermissionService.ValidatePresetPermission("can_archive_warehouse");
        }

        public bool CanArchiveWarehouse { get; }
        public IList<Subdivision> Subdivisions { get; }
    }
}
