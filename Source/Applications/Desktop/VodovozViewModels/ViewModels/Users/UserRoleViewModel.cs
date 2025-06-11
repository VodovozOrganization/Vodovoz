using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.ViewModels.Users
{
	public class UserRoleViewModel : EntityTabViewModelBase<UserRole>
	{
		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IUserRoleRepository _userRoleRepository;
		private readonly IUserRoleService _userRoleService;
		private AvailableDatabase _selectedAvailableDatabase;
		private AvailableDatabase _selectedEntityAvailableDatabase;
		private object[] _selectedPrivileges = Array.Empty<object>();
		private DelegateCommand _addAvailableDatabaseCommand;
		private DelegateCommand _removeAvailableDatabaseCommand;
		private DelegateCommand<PrivilegeType> _addPrivilegeCommand;
		private DelegateCommand _removePrivilegeCommand;
		private DelegateCommand _copyPrivilegeCommand;
		private bool _canCreateRole;

		public UserRoleViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IPrivilegesRepository privilegesRepository,
			IUserRoleRepository userRoleRepository,
			IUserRoleService userRoleService,
			INavigationManager navigation = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(privilegesRepository is null)
			{
				throw new ArgumentNullException(nameof(privilegesRepository));
			}
			
			_userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
			_userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
			PrivilegesNames = privilegesRepository.GetAllPrivilegesNames(UoW);
			AllAvailableDatabases = new GenericObservableList<AvailableDatabase>(GetAllAvailableDatabases());
		}

		public bool CanAddAvailableDatabase => SelectedAvailableDatabase != null;
		public bool CanRemoveAvailableDatabase => SelectedEntityAvailableDatabase != null;
		public bool HasSelectedPrivileges => SelectedPrivileges.Any();

		public AvailableDatabase SelectedAvailableDatabase
		{
			get => _selectedAvailableDatabase;
			set
			{
				if(SetField(ref _selectedAvailableDatabase, value))
				{
					OnPropertyChanged(nameof(CanAddAvailableDatabase));
				}
			}
		}

		public AvailableDatabase SelectedEntityAvailableDatabase
		{
			get => _selectedEntityAvailableDatabase;
			set
			{
				if(SetField(ref _selectedEntityAvailableDatabase, value))
				{
					OnPropertyChanged(nameof(CanRemoveAvailableDatabase));
				}
			}
		}
		
		public object[] SelectedPrivileges
		{
			get => _selectedPrivileges;
			set
			{
				if(SetField(ref _selectedPrivileges, value))
				{
					OnPropertyChanged(nameof(HasSelectedPrivileges));
				}
			}
		}

		public IEnumerable<PrivilegeName> PrivilegesNames { get; }
		public GenericObservableList<AvailableDatabase> AllAvailableDatabases { get; }

		public DelegateCommand AddAvailableDatabaseCommand =>
			_addAvailableDatabaseCommand ?? (_addAvailableDatabaseCommand = new DelegateCommand(
				() =>
				{
					Entity.AddAvailableDatabase(SelectedAvailableDatabase);
					AllAvailableDatabases.Remove(SelectedAvailableDatabase);
					OnPropertyChanged(nameof(CanAddAvailableDatabase));
				}));
		
		public DelegateCommand RemoveAvailableDatabaseCommand =>
			_removeAvailableDatabaseCommand ?? (_removeAvailableDatabaseCommand = new DelegateCommand(
				() =>
				{
					AllAvailableDatabases.Add(SelectedEntityAvailableDatabase);
					Entity.AvailableDatabases.Remove(SelectedEntityAvailableDatabase);
					OnPropertyChanged(nameof(CanRemoveAvailableDatabase));
				}));
		
		public DelegateCommand<PrivilegeType> AddPrivilegeCommand =>
			_addPrivilegeCommand ?? (_addPrivilegeCommand = new DelegateCommand<PrivilegeType>(
				type =>
				{
					var privilege = CreatePrivilegeByType(type);
					Entity.Privileges.Add(privilege);
				}));

		public DelegateCommand RemovePrivilegeCommand =>
			_removePrivilegeCommand ?? (_removePrivilegeCommand = new DelegateCommand(
				() =>
				{
					foreach(var privilege in SelectedPrivileges)
					{
						Entity.Privileges.Remove((PrivilegeBase)privilege);
					}
				}));
		
		public DelegateCommand CopyPrivilegesCommand =>
			_copyPrivilegeCommand ?? (_copyPrivilegeCommand = new DelegateCommand(
				() =>
				{
					foreach(var privilege in SelectedPrivileges)
					{
						var copiedPrivilege = (PrivilegeBase)privilege;
						var newPrivilege = CreatePrivilegeByType(copiedPrivilege.PrivilegeType);
						newPrivilege.PrivilegeName = copiedPrivilege.PrivilegeName;
						Entity.Privileges.Add(newPrivilege);
					}
				}));

		protected override void AfterSave()
		{
			if(Entity.Id > 0)
			{
				return;
			}

			CreateRole();
		}

		private void CreateRole()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				try
				{
					_logger.Debug($"Создаем роль {Entity.Name}");
					_userRoleRepository.CreateUserRoleIfNotExists(uow, Entity.Name);
				}
				catch(Exception e)
				{
					_logger.Error(e, $"Ошибка при создании роли {Entity.Name}");
					ShowWarningMessage("Не удалось создать роль в базе. Для этого необходимы привилегии CREATE USER или CREATE ROLE");
					return;
				}

				try
				{
					_logger.Debug($"Накидываем на нее права");
					_userRoleService.GrantDatabasePrivileges(uow, Entity);
				}
				catch(Exception e)
				{
					_logger.Error(e, $"Ошибка при наполнении роли {Entity.Name} правами");
					ShowWarningMessage("Не удалось наполнить роль всеми привилегиями");
				}
			}
		}

		private PrivilegeBase CreatePrivilegeByType(PrivilegeType type)
		{
			PrivilegeBase privilege;
			switch(type)
			{
				case PrivilegeType.GlobalPrivilege:
					privilege = new GlobalPrivilege();
					break;
				case PrivilegeType.DatabasePrivilege:
					privilege = new DatabasePrivilege();
					break;
				case PrivilegeType.TablePrivilege:
					privilege = new TablePrivilege();
					break;
				case PrivilegeType.SpecialPrivilege:
					privilege = new SpecialPrivilege();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}

			privilege.UserRole = Entity;
			return privilege;
		}
		
		private IList<AvailableDatabase> GetAllAvailableDatabases()
		{
			var availableDatabases = _userRoleRepository.GetAllAvailableDatabases(UoW);

			if(!Entity.AvailableDatabases.Any())
			{
				return availableDatabases;
			}
			
			foreach(var availableDatabase in Entity.AvailableDatabases)
			{
				availableDatabases.Remove(availableDatabase);
			}

			return availableDatabases;
		}
	}
}
