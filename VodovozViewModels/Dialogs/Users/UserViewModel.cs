using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using QS.Commands;
using Vodovoz.ViewModels.Permissions;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.Domain.Permissions.Warehouse;

namespace Vodovoz.ViewModels
{
    public class UserViewModel : EntityTabViewModelBase<User>
    {
		public override bool HasChanges => true;

		public UserViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, IPermissionRepository permissionRepository, ICommonServices commonServices, INavigationManager navigation = null) 
            : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
        {
			_permissionRepository = permissionRepository ?? throw new System.ArgumentNullException(nameof(permissionRepository));
		}

		public bool CanEditLogin => UoW.IsNew;

		private PresetUserPermissionsViewModel _presetUserPermissionsViewModel;
		public PresetUserPermissionsViewModel PresetPermissionsViewModel
		{
			get
			{
				if(_presetUserPermissionsViewModel == null)
				{
					_presetUserPermissionsViewModel = new PresetUserPermissionsViewModel(UoW, _permissionRepository, Entity);
				}

				return _presetUserPermissionsViewModel;
			}
		}

		private WarehousePermissionsViewModel _warehousePermissionsViewModel;
		public WarehousePermissionsViewModel WarehousePermissionsViewModel
		{
			get
			{
				if(_warehousePermissionsViewModel == null)
				{
					var model = new UserWarehousePermissionModel(UoW, Entity);
					_warehousePermissionsViewModel = new WarehousePermissionsViewModel(UoW, model);
					_warehousePermissionsViewModel.CanEdit = true;
				}

				return _warehousePermissionsViewModel;
			}
		}

		private DelegateCommand _saveCommand;
		public DelegateCommand SaveCommand
		{
			get
			{
				if(_saveCommand == null)
				{
					_saveCommand = new DelegateCommand(() => {
						if(!Validate())
						{
							return;
						}
						_warehousePermissionsViewModel.SaveWarehousePermissions();
						PresetPermissionsViewModel.SaveCommand.Execute();
						UoW.Save();
						Close(false, CloseSource.Save);
					});
				}
				return _saveCommand;
			}
		}

		private DelegateCommand _cancelCommand;
		private readonly IPermissionRepository _permissionRepository;

		public DelegateCommand CancelCommand
		{
			get
			{
				if(_cancelCommand == null)
				{
					_cancelCommand = new DelegateCommand(() => {
						Close(true, CloseSource.Cancel);
					});
				}
				return _cancelCommand;
			}
		}
	}
}
