using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using QS.Commands;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.Project.Journal;
using Vodovoz.Controllers;
using Vodovoz.Domain.Permissions;
using Vodovoz.ViewModels.Permissions;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Journals;

namespace Vodovoz.ViewModels
{
    public class UserViewModel : EntityTabViewModelBase<User>
    {
		private readonly ILifetimeScope _scope;
		private readonly IUserPermissionsController _userPermissionsController;

		private PresetUserPermissionsViewModel _presetUserPermissionsViewModel;
		private WarehousePermissionsViewModel _warehousePermissionsViewModel;
		private DelegateCommand _saveCommand;
		private DelegateCommand _cancelCommand;
		private DelegateCommand _addPermissionsToUserCommand;
		private DelegateCommand _changePermissionsFromUserCommand;
		
		public UserViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ILifetimeScope scope) 
            : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_userPermissionsController = _scope.Resolve<IUserPermissionsController>();
		}

		public event Action<IList<UserPermissionNode>> UpdateEntityUserPermissionsAction;
		public event Action<IList<EntitySubdivisionForUserPermission>> UpdateEntitySubdivisionForUserPermissionsAction;
		public event Action UpdateWarehousePermissionsAction;

		public bool CanEditLogin => UoW.IsNew;
		public override bool HasChanges => true;
		
		public PresetUserPermissionsViewModel PresetPermissionsViewModel
		{
			get
			{
				if(_presetUserPermissionsViewModel == null)
				{
					InitializePresetUserPermissionsViewModel();
				}

				return _presetUserPermissionsViewModel;
			}
		}

		public WarehousePermissionsViewModel WarehousePermissionsViewModel
		{
			get
			{
				if(_warehousePermissionsViewModel == null)
				{
					InitializeWarehousePermissionsViewModel();
				}

				return _warehousePermissionsViewModel;
			}
		}

		public DelegateCommand SaveCommand =>
			_saveCommand ?? (_saveCommand = new DelegateCommand(() =>
			{
				if(!Validate())
				{
					return;
				}

				_warehousePermissionsViewModel.SaveWarehousePermissions();
				PresetPermissionsViewModel.SaveCommand.Execute();
				UoW.Save();
				Close(false, CloseSource.Save);
			}));

		public DelegateCommand CancelCommand => _cancelCommand ?? (
			_cancelCommand = new DelegateCommand(
				() =>
				{
					Close(true, CloseSource.Cancel);
				}));

		public DelegateCommand AddPermissionsToUserCommand => _addPermissionsToUserCommand ?? (
			_addPermissionsToUserCommand = new DelegateCommand(
				() =>
				{
					var userJournal = CreateSelectUserJournalAndOpenAsSlave();
					userJournal.OnEntitySelectedResult += (sender, args) =>
					{
						var selectedNode = args.SelectedNodes.FirstOrDefault();
						
						if(selectedNode == null)
						{
							return;
						}
						if(selectedNode.Id == Entity.Id)
						{
							ShowWarningMessage("Выбран тот же самый пользователь. Выберите другого");
							return;
						}
						
						_userPermissionsController.AddingPermissionsToUser(UoW, selectedNode.Id, Entity.Id);
						UpdateUserPermissionsData();
					};
				}));

		public DelegateCommand ChangePermissionsFromUserCommand => _changePermissionsFromUserCommand ?? (
			_changePermissionsFromUserCommand = new DelegateCommand(
				() =>
				{
					var userJournal = CreateSelectUserJournalAndOpenAsSlave();
					userJournal.OnEntitySelectedResult += (sender, args) =>
					{
						var selectedNode = args.SelectedNodes.FirstOrDefault();
						
						if(selectedNode == null)
						{
							return;
						}
						if(selectedNode.Id == Entity.Id)
						{
							ShowWarningMessage("Выбран тот же самый пользователь. Выберите другого");
							return;
						}

						_userPermissionsController.ChangePermissionsFromUser(UoW, selectedNode.Id, Entity.Id);
						UpdateUserPermissionsData();
					};
				}));

		private SelectUserJournalViewModel CreateSelectUserJournalAndOpenAsSlave()
		{
			var userJournal = _scope.Resolve<SelectUserJournalViewModel>();
			userJournal.SelectionMode = JournalSelectionMode.Single;
			TabParent.AddSlaveTab(this, userJournal);
			return userJournal;
		}

		private void UpdateUserPermissionsData()
		{
			UpdateEntityUserPermissionsAction?.Invoke(_userPermissionsController.GetAllNewEntityUserPermissions());
			PresetPermissionsViewModel.UpdateData(_userPermissionsController.NewUserPresetPermissions);
			UpdateEntitySubdivisionForUserPermissionsAction?.Invoke(_userPermissionsController.NewEntitySubdivisionForUserPermissions);
			WarehousePermissionsViewModel.UpdateData(_userPermissionsController.NewUserWarehousesPermissions);
			UpdateWarehousePermissionsAction?.Invoke();
		}
		
		private void InitializeWarehousePermissionsViewModel()
		{
			var model = _scope.Resolve<UserWarehousePermissionModel>(
				new TypedParameter(typeof(IUnitOfWork), UoW),
				new TypedParameter(typeof(User), Entity));
			_warehousePermissionsViewModel = _scope.Resolve<WarehousePermissionsViewModel>(
				new TypedParameter(typeof(IUnitOfWork), UoW),
				new TypedParameter(typeof(WarehousePermissionModelBase), model));
			_warehousePermissionsViewModel.CanEdit = true;
		}
		
		private void InitializePresetUserPermissionsViewModel()
		{
			_presetUserPermissionsViewModel = _scope.Resolve<PresetUserPermissionsViewModel>(
				new TypedParameter(typeof(IUnitOfWork), UoW),
				new TypedParameter(typeof(User), Entity));
		}
	}
}
