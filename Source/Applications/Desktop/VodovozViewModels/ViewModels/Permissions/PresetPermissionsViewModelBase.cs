using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Permissions;
using QS.ViewModels;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.ViewModels.Permissions
{
	public abstract class PresetPermissionsViewModelBase : UoWWidgetViewModelBase
	{
		private HierarchicalPresetPermissionBase _selectedHierarchicalPresetPermissionBase;
		private PresetUserPermissionSource _selectedPresetUserPermissionSource;
		
		protected IPermissionRepository permissionRepository;
		private readonly UsersPresetPermissionValuesGetter _usersPresetPermissionValuesGetter;
		private readonly UserPermissionsExporter _userPermissionsExporter;

		protected IList<HierarchicalPresetPermissionBase> deletePermissionList = new List<HierarchicalPresetPermissionBase>();

		protected List<HierarchicalPresetPermissionBase> permissionList;
		public GenericObservableList<HierarchicalPresetPermissionBase> ObservablePermissionsList { get; protected set; }

		protected List<PresetUserPermissionSource> originalPermissionsSourceList;
		public GenericObservableList<PresetUserPermissionSource> ObservablePermissionsSourceList { get; protected set; }

		protected PresetPermissionsViewModelBase(
			IUnitOfWork unitOfWork,
			IPermissionRepository permissionRepository,
			UsersPresetPermissionValuesGetter usersPresetPermissionValuesGetter,
			UserPermissionsExporter userPermissionsExporter)
		{
			UoW = unitOfWork ?? throw new NullReferenceException(nameof(unitOfWork));
			this.permissionRepository = permissionRepository ?? throw new NullReferenceException(nameof(permissionRepository));
			_usersPresetPermissionValuesGetter =
				usersPresetPermissionValuesGetter ?? throw new ArgumentNullException(nameof(usersPresetPermissionValuesGetter));
			_userPermissionsExporter = userPermissionsExporter ?? throw new ArgumentNullException(nameof(userPermissionsExporter));

			CreateCommands();
		}

		protected void OrderPermission()
		{
			permissionList.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal));
			originalPermissionsSourceList.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal));
		}

		public virtual void StartSearch(string searchstring) { }

		protected DelegateCommand addPermissionCommand;
		public virtual DelegateCommand AddPermissionCommand { get; }

		protected DelegateCommand removePermissionCommand;
		public virtual DelegateCommand RemovePermissionCommand { get; }

		protected DelegateCommand saveCommand;
		public virtual DelegateCommand SaveCommand { get; }
		
		protected DelegateCommand getUsersWithActiveSelectedAvailablePermissionCommand;

		public virtual DelegateCommand GetUsersWithActiveSelectedAvailablePermissionCommand =>
			getUsersWithActiveSelectedAvailablePermissionCommand ?? (getUsersWithActiveSelectedAvailablePermissionCommand =
				new DelegateCommand(
					() => ExportCommand.Execute((SelectedPresetUserPermissionSource.Name, SelectedPresetUserPermissionSource.DisplayName))
				));
		
		protected DelegateCommand getUsersWithActiveSelectedCurrentPermissionCommand;

		public virtual DelegateCommand GetUsersWithActiveSelectedCurrentPermissionCommand =>
			getUsersWithActiveSelectedCurrentPermissionCommand ?? (getUsersWithActiveSelectedCurrentPermissionCommand = new DelegateCommand(
				() => ExportCommand.Execute(
					(SelectedHierarchicalPresetPermissionBase.PermissionName, SelectedHierarchicalPresetPermissionBase.DisplayName))	
			));

		private DelegateCommand<(string PermissionName, string PermissionTitle)> ExportCommand { get; set; }

		public HierarchicalPresetPermissionBase SelectedHierarchicalPresetPermissionBase
		{
			get => _selectedHierarchicalPresetPermissionBase;
			set => SetField(ref _selectedHierarchicalPresetPermissionBase, value);
		}
		
		public PresetUserPermissionSource SelectedPresetUserPermissionSource
		{
			get => _selectedPresetUserPermissionSource;
			set => SetField(ref _selectedPresetUserPermissionSource, value);
		}
		
		private void CreateCommands()
		{
			ExportCommand = new DelegateCommand<(string PermissionName, string PermissionTitle)>(
				permission =>
				{
					var usersWithActivePermission =
						_usersPresetPermissionValuesGetter.GetUsersWithActivePermission(UoW, permission.PermissionName);
					
					var usersWithActivePermissionBySubdivision =
						_usersPresetPermissionValuesGetter.GetUsersWithActivePermissionPresetByOwnSubdivision(UoW, permission.PermissionName);

					_userPermissionsExporter.ExportUsersPresetPermissionToExcel(
						permission, usersWithActivePermission, usersWithActivePermissionBySubdivision);
				}
			);
		}
	}
}
