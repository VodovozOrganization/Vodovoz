using Autofac;
using QS.Commands;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Permissions;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.Journals;
using Vodovoz.Services;
using Vodovoz.Settings.Organizations;
using Vodovoz.ViewModels.Permissions;

namespace Vodovoz.ViewModels
{
	public class UserViewModel : EntityTabViewModelBase<User>
	{
		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IUserRoleRepository _userRoleRepository;
		private readonly IUserRoleService _userRoleService;
		private readonly ILifetimeScope _scope;
		private readonly IUserPermissionsController _userPermissionsController;
		private readonly UserRole _oldCurrentUserRole;
		private readonly IList<UserRole> _rolesToRevoke = new List<UserRole>();
		private readonly IList<UserRole> _rolesToGrant = new List<UserRole>();

		private UserRole _selectedAvailableUserRole;
		private UserRole _selectedUserRole;
		private PresetUserPermissionsViewModel _presetUserPermissionsViewModel;
		private WarehousePermissionsViewModel _warehousePermissionsViewModel;
		private DelegateCommand _saveCommand;
		private DelegateCommand _cancelCommand;
		private DelegateCommand _addPermissionsToUserCommand;
		private DelegateCommand _changePermissionsFromUserCommand;
		private DelegateCommand _addUserRoleToUserCommand;
		private DelegateCommand _removeUserRoleCommand;
		private IEnumerable<string> _userGrants;

		public UserViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IUserRoleRepository userRoleRepository,
			IUserRoleService userRoleService,
			ILifetimeScope scope) 
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
			_userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_userPermissionsController = _scope.Resolve<IUserPermissionsController>();

			var allAvailableUserRoles = _userRoleRepository.GetAllUserRoles(UoW);
			GetUserGrants();
			UpdateCurrentUserRole(allAvailableUserRoles);
			GetUserRoles(allAvailableUserRoles);
			AvailableUserRoles = new GenericObservableList<UserRole>(GetAvailableUserRoles(allAvailableUserRoles));
			_oldCurrentUserRole = Entity.CurrentUserRole;
			IsSameUser = CommonServices.UserService.CurrentUserId == Entity.Id;
			
			ConfigureEntityChangingRelations();
			SubscribeUpdateOnChanges();
		}

		public event Action<IList<UserPermissionNode>> UpdateEntityUserPermissionsAction;
		public event Action<IList<EntitySubdivisionForUserPermission>> UpdateEntitySubdivisionForUserPermissionsAction;
		public event Action UpdateWarehousePermissionsAction;
		public event Action UpdateUserRolesForCurrentRoleAction;

		public bool CanEditLogin => UoW.IsNew;
		public bool IsSameUser { get; }
		public bool HasUserOnServer { get; private set; }
		
		public GenericObservableList<UserRole> AvailableUserRoles { get; }
		public bool HasCurrentUserRole => Entity.CurrentUserRole != null;

		public string UserRoleDescription
		{
			get => Entity.CurrentUserRole?.Description;
			set => Entity.CurrentUserRole.Description = value;
		}
		
		public UserRole SelectedAvailableUserRole
		{
			get => _selectedAvailableUserRole;
			set
			{
				if(SetField(ref _selectedAvailableUserRole, value))
				{
					OnPropertyChanged(nameof(CanAddUserRoleToUser));
				}
			}
		}

		public UserRole SelectedUserRole
		{
			get => _selectedUserRole;
			set
			{
				if(SetField(ref _selectedUserRole, value))
				{
					OnPropertyChanged(nameof(CanRemoveUserRole));
				}
			}
		}
		
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

				UpdateUserRoles();
				UpdateCurrentUserRole();

				Close(false, CloseSource.Save);
			}));

		private void GetUserGrants()
		{
			try
			{
				_userGrants = _userRoleRepository.ShowGrantsForUser(UoW, Entity.Login);
				HasUserOnServer = true;
			}
			catch(Exception e)
			{
				_logger.Error(e, $"Ошибка при проверке прав у пользователя {Entity.Name}, логин {Entity.Login}");
				_userGrants = Array.Empty<string>();
				ShowErrorMessage("Произошла ошибка.\nСкорее всего пользователь не зарегистрирован на сервере\n" +
					"Вкладка откроется после закрытия информационного окна");
				HasUserOnServer = false;
			}
		}
		
		private void UpdateUserRoles()
		{
			var devSubdivisionId = _scope.Resolve<ISubdivisionSettings>().GetDevelopersSubdivisionId;
			var isDeveloper = _scope.Resolve<IEmployeeService>().GetEmployeeForUser(UoW, CurrentUser.Id).Subdivision.Id == devSubdivisionId;
			
			try
			{
				foreach(var role in _rolesToGrant)
				{
					_userRoleRepository.GrantRoleToUser(UoW, role.Name, Entity.Login, isDeveloper);
				}
				foreach(var role in _rolesToRevoke)
				{
					_userRoleRepository.RevokeRoleFromUser(UoW, role.Name, Entity.Login);
				}
			}
			catch(Exception e)
			{
				_logger.Error(e, $"Ошибка при обновлении доступных ролей у пользователя {Entity.Name}");
				ShowErrorMessage("Ошибка при обновлении доступных ролей у пользователя");
			}
		}

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
		
		public DelegateCommand AddUserRoleToUserCommand =>
			_addUserRoleToUserCommand ?? (_addUserRoleToUserCommand = new DelegateCommand(
				() =>
				{
					Entity.UserRoles.Add(SelectedAvailableUserRole);
					_rolesToGrant.Add(SelectedAvailableUserRole);
					_rolesToRevoke.Remove(SelectedAvailableUserRole);
					AvailableUserRoles.Remove(SelectedAvailableUserRole);
					UpdateUserRolesForCurrentRoleAction?.Invoke();
					OnPropertyChanged(nameof(CanAddUserRoleToUser));
				},
				() => CanAddUserRoleToUser)
			);
		
		public DelegateCommand RemoveUserRoleCommand =>
			_removeUserRoleCommand ?? (_removeUserRoleCommand = new DelegateCommand(
				() =>
				{
					AvailableUserRoles.Add(SelectedUserRole);
					_rolesToRevoke.Add(SelectedUserRole);
					_rolesToGrant.Remove(SelectedUserRole);
					Entity.UserRoles.Remove(SelectedUserRole);
					UpdateUserRolesForCurrentRoleAction?.Invoke();
					OnPropertyChanged(nameof(CanRemoveUserRole));
				},
				() => CanRemoveUserRole)
			);

		public void UpdateChanges(object sender, EventArgs e) => HasChanges = true;

		public void ExportPermissions((string PermissionName, string PermissionTitle) permission)
		{
			var permissionValuesGetter = _scope.Resolve<UsersEntityPermissionValuesGetter>();
			var permissionsExporter = _scope.Resolve<UserPermissionsExporter>();

			var usersWithPermission = permissionValuesGetter.GetUsersWithEntityPermission(UoW, permission.PermissionName);
			var usersWithPermissionBySubdivisions =
				permissionValuesGetter.GetUsersWithActivePermissionPresetByOwnSubdivision(UoW, permission.PermissionName);

			permissionsExporter.ExportUsersEntityPermissionToExcel(permission, usersWithPermission, usersWithPermissionBySubdivisions);
		}

		private bool CanAddUserRoleToUser => SelectedAvailableUserRole != null;
		private bool CanRemoveUserRole => SelectedUserRole != null && SelectedUserRole.Id != Entity.CurrentUserRole?.Id;

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(u => u.CurrentUserRole, () => HasCurrentUserRole);
			SetPropertyChangeRelation(u => u.CurrentUserRole, () => UserRoleDescription);
		}

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

		private void UpdateCurrentUserRole(IList<UserRole> allAvailableUserRoles)
		{
			var defaultRole = _userGrants.SingleOrDefault(x => x.Contains("SET DEFAULT ROLE"));

			if(string.IsNullOrWhiteSpace(defaultRole))
			{
				return;
			}
			
			var roleName = defaultRole.Split(' ')[3].Trim('`');
			Entity.CurrentUserRole = allAvailableUserRoles.SingleOrDefault(x => x.Name == roleName);
		}

		private void GetUserRoles(IList<UserRole> allAvailableUserRoles)
		{
			foreach(var userGrant in _userGrants)
			{
				var matches = Regex.Matches(userGrant, _userRoleService.SearchingPatternFromUserGrants(Entity.Login));

				if(matches.Count <= 0)
				{
					continue;
				}

				var userRoleName = matches[0].Groups[1].Value;
				var userRole = allAvailableUserRoles.SingleOrDefault(x => x.Name == userRoleName);
					
				if(userRole != null)
				{
					Entity.UserRoles.Add(userRole);
				}
			}
		}

		private IList<UserRole> GetAvailableUserRoles(IList<UserRole> allAvailableUserRoles)
		{
			if(!Entity.UserRoles.Any())
			{
				return allAvailableUserRoles;
			}
			
			foreach(var userRole in Entity.UserRoles)
			{
				allAvailableUserRoles.Remove(userRole);
			}

			return allAvailableUserRoles;
		}
		
		private void UpdateCurrentUserRole()
		{
			try
			{
				if(_oldCurrentUserRole == Entity.CurrentUserRole)
				{
					return;
				}

				if(Entity.CurrentUserRole.Name != UserRoles.User
					&& Entity.CurrentUserRole.Name != UserRoles.Financier)
				{
					_userRoleRepository.GrantRoleToUser(UoW, UserRoles.User, Entity.Login, true);
				}
				else
				{
					_userRoleRepository.RevokeRoleFromUser(UoW, UserRoles.User, Entity.Login);
					_userRoleRepository.GrantRoleToUser(UoW, UserRoles.User, Entity.Login);
				}
				_userRoleRepository.SetDefaultRoleToUser(UoW, Entity.CurrentUserRole, Entity.Login);
			}
			catch(Exception e)
			{
				ShowErrorMessage("При установке роли пользователя по умолчанию произошла ошибка. Возможно не хватает прав.");
				_logger.Error(e, "Ошибка при установке роли по умолчанию");
			}
		}
		
		private void SubscribeUpdateOnChanges()
		{
			Entity.PropertyChanged += UpdateChanges;
			AvailableUserRoles.ListContentChanged += UpdateChanges;
			Entity.UserRoles.CollectionChanged += UpdateChanges;
			PresetPermissionsViewModel.ObservablePermissionsList.ListContentChanged += UpdateChanges;
			
			foreach(var warehousePermissionNode in WarehousePermissionsViewModel.AllWarehouses)
			{
				warehousePermissionNode.SubNodeViewModel.ListContentChanged += UpdateChanges;
			}
		}
		
		private void UnsubscribeUpdateOnChanges()
		{
			Entity.PropertyChanged -= UpdateChanges;
			AvailableUserRoles.ListContentChanged -= UpdateChanges;
			Entity.UserRoles.CollectionChanged -= UpdateChanges;
			PresetPermissionsViewModel.ObservablePermissionsList.ListContentChanged -= UpdateChanges;
			
			foreach(var warehousePermissionNode in WarehousePermissionsViewModel.AllWarehouses)
			{
				warehousePermissionNode.SubNodeViewModel.ListContentChanged -= UpdateChanges;
			}
		}

		public override void Dispose()
		{
			UnsubscribeUpdateOnChanges();
			base.Dispose();
		}
	}
}
