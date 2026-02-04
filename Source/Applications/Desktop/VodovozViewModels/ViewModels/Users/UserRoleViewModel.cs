using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.Errors.Users;

namespace Vodovoz.ViewModels.Users
{
	public class UserRoleViewModel : EntityTabViewModelBase<UserRole>
	{
		private readonly ILogger<UserRoleViewModel> _logger;
		private readonly IUserRoleRepository _userRoleRepository;
		private readonly IUserRoleService _userRoleService;
		private readonly IInteractiveService _interactiveService;
		private AvailableDatabase _selectedAvailableDatabase;
		private AvailableDatabase _selectedEntityAvailableDatabase;
		private object[] _selectedPrivileges = Array.Empty<object>();
		private bool _canCreateRole;

		public UserRoleViewModel(
			ILogger<UserRoleViewModel> logger,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IPrivilegesRepository privilegesRepository,
			IUserRoleRepository userRoleRepository,
			IUserRoleService userRoleService,
			IInteractiveService interactiveService,
			INavigationManager navigation = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(privilegesRepository is null)
			{
				throw new ArgumentNullException(nameof(privilegesRepository));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
			_userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			PrivilegesNames = privilegesRepository.GetAllPrivilegesNames(UoW);
			AllAvailableDatabases = new GenericObservableList<AvailableDatabase>(GetAllAvailableDatabases());

			InitializeCommands();
		}

		public bool CanAddAvailableDatabase => SelectedAvailableDatabase != null;
		public bool CanRemoveAvailableDatabase => SelectedEntityAvailableDatabase != null;
		public bool HasSelectedPrivileges => SelectedPrivileges.Any();
		public bool CanEditRoleName => Entity.Id == 0;

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

		public DelegateCommand SaveCommand { get; private set; }
		public DelegateCommand CancelCommand { get; private set; }
		public DelegateCommand AddAvailableDatabaseCommand { get; private set; }
		public DelegateCommand RemoveAvailableDatabaseCommand { get; private set; }
		public DelegateCommand<PrivilegeType> AddPrivilegeCommand { get; private set; }
		public DelegateCommand RemovePrivilegeCommand { get; private set; }
		public DelegateCommand CopyPrivilegesCommand { get; private set; }
		private IList<PrivilegeBase> AddedPrivileges { get; } = new List<PrivilegeBase>();
		private IList<PrivilegeBase> RemovedPrivileges { get; } = new List<PrivilegeBase>();

		protected override bool BeforeSave()
		{
			UoW.OpenTransaction();
			
			if(Entity.Id > 0)
			{
				UpdateRole();
				return true;
			}

			CreateRole();
			return true;
		}
		
		private void InitializeCommands()
		{
			SaveCommand = new DelegateCommand(SaveAndClose);
			CancelCommand = new DelegateCommand(CloseFromCancel);
			AddAvailableDatabaseCommand = new DelegateCommand(AddAvailableDatabase);
			RemoveAvailableDatabaseCommand = new DelegateCommand(RemoveAvailableDatabase);
			AddPrivilegeCommand = new DelegateCommand<PrivilegeType>(AddNewPrivilege);
			RemovePrivilegeCommand = new DelegateCommand(RemovePrivilege);
			CopyPrivilegesCommand = new DelegateCommand(CopyPrivileges);
		}
		
		private void SynchronizeRoleWithServer()
		{
			var result = TryCheckRoleFromServer(out var rolePrivilegesFromServer);

			if(result.IsSuccess)
			{
				return;
			}

			if(result.IsFailure && result.Errors.First().Code == "UserRoleFromServerNotExists")
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Данная роль не найдена на сервере. Создаем по текущим настройкам");
				UoW.OpenTransaction();
				
				CreateRole();
				UoW.Save();
				
				return;
			}

			var answer = _interactiveService.Question(
				new []
				{
					"С сервера",
					"С программы"
				},
				"На сервере и в программе данная роль с разными привилегиями. Откуда взять данные за основу?"
			);

			UoW.OpenTransaction();
			
			if(answer == "С сервера")
			{
				Entity.Privileges.Clear();

				var privilegesBySchema = rolePrivilegesFromServer[Entity.Name];
				foreach(var groupedPrivileges in privilegesBySchema)
				{
					var privilegeType = groupedPrivileges.Key.ToPrivilegeType();
					
					foreach(var privilege in groupedPrivileges.Value)
					{
						if(privilege == "USAGE")
						{
							continue;
						}
						
						var privilegeName = PrivilegesNames
							.FirstOrDefault(p => p.Name == privilege && p.PrivilegeType == privilegeType);

						if(privilegeName is null)
						{
							throw new AbortCreatingPageException("Что-то пошло не так, обратитесь в отдел разработки", "Ошибка");
						}

						var schemaAndTable = groupedPrivileges.Key.Split('.');
						var newPrivilege = CreatePrivilegeByType(privilegeType);
						newPrivilege.PrivilegeName = privilegeName;
						newPrivilege.DatabaseName = schemaAndTable[0];
						newPrivilege.TableName = schemaAndTable[1];
						Entity.Privileges.Add(newPrivilege);
					}
				}
			}
			else
			{
				if(string.IsNullOrWhiteSpace(answer))
				{
					_interactiveService.ShowMessage(ImportanceLevel.Info, "Устанавливаем права с программы");
				}
				
				_logger.LogDebug("Отзываем права на сервере...");
				_userRoleRepository.RevokeAllPrivilegesFromRole(UoW, Entity.Name);
				_logger.LogDebug("Накидываем права с программы...");
				_userRoleService.GrantDatabasePrivileges(UoW, Entity);
			}
			
			_logger.LogDebug("Сохраняем изменения после синхронизации...");
			UoW.Save();
		}
		
		private Result TryCheckRoleFromServer(out IDictionary<string, IDictionary<string, IList<string>>> rolePrivilegesFromServer)
		{
			if(Entity.Id == 0)
			{
				rolePrivilegesFromServer = new Dictionary<string, IDictionary<string, IList<string>>>();
				return Result.Success();
			}
			
			rolePrivilegesFromServer = _userRoleService.GetPrivilegesFromRoleInDatabase(UoW, Entity.Name);
			if(!rolePrivilegesFromServer.TryGetValue(Entity.Name, out var privilegesFromServer))
			{
				return Result.Failure(UserRoleErrors.UserRoleFromServerNotExists());
			}

			var programRole = Entity.Privileges.ToLookup(
				x => $"{x.DatabaseName}.{x.TableName}", y => y.PrivilegeName.Name);
			
			foreach(var groupedPrivileges in programRole)
			{
				if(!privilegesFromServer.TryGetValue(groupedPrivileges.Key, out var privileges))
				{
					return Result.Failure(UserRoleErrors.PrivilegesOnThisSchemaOrDatabaseFromServerNotExists());
				}

				//убираем ничего не значащую привилегию USAGE 
				privileges.Remove("USAGE");
				
				if(privileges.Count != groupedPrivileges.Count())
				{
					return Result.Failure(UserRoleErrors.CountPrivilegesFromServerAndProgramNotEqual());
				}
				
				var sortedPrivileges = groupedPrivileges.OrderBy(x => x).ToList();

				for(var i = 0; i < privileges.Count; i++)
				{
					if(!string.Equals(privileges[i], sortedPrivileges[i], StringComparison.CurrentCultureIgnoreCase))
					{
						return Result.Failure(UserRoleErrors.PrivilegesFromServerAndProgramNotSame());
					}
				}
			}
			
			return Result.Success();
		}

		private void UpdateRole()
		{
			foreach(var privilege in AddedPrivileges)
			{
				_userRoleRepository.GrantPrivilegeToRole(UoW, privilege.ToString(), Entity.Name);
			}
			
			foreach(var privilege in RemovedPrivileges)
			{
				_userRoleRepository.RevokePrivilegeFromRole(UoW, privilege.ToString(), Entity.Name);
			}
		}

		private void CreateRole()
		{
			_logger.LogDebug("Создаем роль {Role}", Entity.Name);
			_userRoleRepository.CreateUserRoleIfNotExists(UoW, Entity.Name);

			_logger.LogDebug("Накидываем на нее права");
			_userRoleService.GrantDatabasePrivileges(UoW, Entity);
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

		private void CloseFromCancel()
		{
			Close(false, CloseSource.Cancel);
		}
		
		private void AddAvailableDatabase()
		{
			Entity.AddAvailableDatabase(SelectedAvailableDatabase);
			AllAvailableDatabases.Remove(SelectedAvailableDatabase);
			OnPropertyChanged(nameof(CanAddAvailableDatabase));
		}

		private void RemoveAvailableDatabase()
		{
			AllAvailableDatabases.Add(SelectedEntityAvailableDatabase);
			Entity.AvailableDatabases.Remove(SelectedEntityAvailableDatabase);
			OnPropertyChanged(nameof(CanRemoveAvailableDatabase));
		}

		private void AddNewPrivilege(PrivilegeType type)
		{
			AddPrivilege(type);
		}
		
		private PrivilegeBase AddPrivilege(PrivilegeType type)
		{
			var privilege = CreatePrivilegeByType(type);
			Entity.Privileges.Add(privilege);
			AddedPrivileges.Add(privilege);

			if(RemovedPrivileges.Contains(privilege))
			{
				RemovedPrivileges.Remove(privilege);
			}

			return privilege;
		}

		private void RemovePrivilege()
		{
			foreach(PrivilegeBase privilege in SelectedPrivileges)
			{
				Entity.Privileges.Remove(privilege);
				
				if(AddedPrivileges.Contains(privilege))
				{
					AddedPrivileges.Remove(privilege);
				}
				else
				{
					RemovedPrivileges.Add(privilege);
				}
			}
		}

		private void CopyPrivileges()
		{
			foreach(PrivilegeBase privilege in SelectedPrivileges)
			{
				var copied = AddPrivilege(privilege.PrivilegeType);
				copied.PrivilegeName = privilege.PrivilegeName;
			}
		}
	}
}
