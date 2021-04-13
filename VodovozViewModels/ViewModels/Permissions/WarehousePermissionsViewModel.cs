using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Permissions.Warehouse;
using Vodovoz.ViewModels.ViewModels.PermissionNode;

namespace Vodovoz.ViewModels.Permissions
{
    public delegate void PermissionChanged();

    public class WarehousePermissionsViewModel : TabViewModelBase
    {
        public IUnitOfWork UoW;

        private readonly ICommonServices commonServices;

        public WarehousePermissionsViewModel(INavigationManager navigationManager, ICommonServices commonServices) :
            base(commonServices.InteractiveService, navigationManager)
        {
            this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
            CreateUoW();
        }

        private WarehousePermissionModel warehousePermissionModel { get; set; }

        private IEnumerable<PermissionTypeAllNodeViewModel> allWarehouses;
        public IEnumerable<PermissionTypeAllNodeViewModel> AllWarehouses
        {
            get => allWarehouses;
            set => SetField(ref allWarehouses, value);
        }

        private IEnumerable<WarehouseAllNodeViewModel> allPermissionTypes;
        public IEnumerable<WarehouseAllNodeViewModel> AllPermissionTypes
        {
            get => allPermissionTypes;
            set => SetField(ref allPermissionTypes, value);
        }

        public event PermissionChanged PermChanged;

        private void CreateUoW() => UoW = UnitOfWorkFactory.CreateWithoutRoot();
    }
}