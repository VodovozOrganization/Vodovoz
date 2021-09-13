using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using FluentNHibernate.Conventions;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Permissions;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.ViewModels.Permissions
{
	public class PresetUserPermissionsViewModel: PresetPermissionsViewModelBase
	{
		private Domain.Employees.User user;

		public PresetUserPermissionsViewModel(IUnitOfWork unitOfWork, IPermissionRepository permissionRepository, User user) : base(unitOfWork, permissionRepository)
		{
			this.user = user ?? throw new ArgumentNullException(nameof(user));

			permissionList = permissionRepository.GetAllPresetUserPermission(UoW, user).OfType<HierarchicalPresetPermissionBase>().ToList();
			ObservablePermissionsList = new GenericObservableList<HierarchicalPresetPermissionBase>(permissionList);

			originalPermissionsSourceList = PermissionsSettings.PresetPermissions.Values.ToList();
			foreach (var item in permissionList) 
			{ 
				var sourceItem = originalPermissionsSourceList.SingleOrDefault(x => x.Name == item.PermissionName);
				if (sourceItem != null)
				{
					originalPermissionsSourceList.Remove(sourceItem);
				}
			}
			ObservablePermissionsSourceList = new GenericObservableList<PresetUserPermissionSource>(originalPermissionsSourceList);

			OrderPermission();
		}
		
		public override void StartSearch(string searchString)
		{
			permissionList = permissionRepository.GetAllPresetUserPermission(UoW, user).OfType<HierarchicalPresetPermissionBase>().ToList();
			originalPermissionsSourceList = PermissionsSettings.PresetPermissions.Values.ToList();
			foreach (var item in permissionList) 
			{ 
				var sourceItem = originalPermissionsSourceList.SingleOrDefault(x => x.Name == item.PermissionName);
				if(sourceItem != null)
				{
					originalPermissionsSourceList.Remove(sourceItem);
				}
			}

			ObservablePermissionsSourceList = null;
			ObservablePermissionsSourceList = new GenericObservableList<PresetUserPermissionSource>(originalPermissionsSourceList);
			ObservablePermissionsList = null;
			ObservablePermissionsList = new GenericObservableList<HierarchicalPresetPermissionBase>(permissionList);
			
			if(!searchString.IsEmpty())
			{
				for(int i = 0; i < ObservablePermissionsSourceList.Count; i++)
				{
					if (ObservablePermissionsSourceList[i].DisplayName.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) == -1)
					{
						ObservablePermissionsSourceList.Remove(ObservablePermissionsSourceList[i]);
						i--;
					}
				}
				for(int i = 0; i < ObservablePermissionsList.Count; i++)
				{
					if (ObservablePermissionsList[i].DisplayName.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) == -1)
					{
						ObservablePermissionsList.Remove(ObservablePermissionsList[i]);
						i--;
					}
				}
			}
		}

		public override DelegateCommand<PresetUserPermissionSource> AddPermissionCommand {
			get {
				if(addPermissionCommand == null) {
					addPermissionCommand = new DelegateCommand<PresetUserPermissionSource>(
						(source) => {
							var newPermission = new HierarchicalPresetUserPermission {
								PermissionName = source.Name,
								Value = true,
								User = user
							};
							ObservablePermissionsList.Add(newPermission);

							var sourceItem = ObservablePermissionsSourceList
                                                .SingleOrDefault(x => x.Name == newPermission.PermissionName);
                            ObservablePermissionsSourceList.Remove(sourceItem);

							var deletedPermission = deletePermissionList.FirstOrDefault(x => x.PermissionName == source.Name);
							if(deletedPermission != null)
								deletePermissionList.Remove(deletedPermission);

							OrderPermission();
						},
						(source) => true
					);
				}
				return addPermissionCommand;
			}
		}

		public override DelegateCommand<HierarchicalPresetPermissionBase> RemovePermissionCommand {
			get {
				if(removePermissionCommand == null) {
					removePermissionCommand = new DelegateCommand<HierarchicalPresetPermissionBase>(
						(permissionBase) => {
							var permission = permissionBase as HierarchicalPresetUserPermission;
							ObservablePermissionsList.Remove(permission);
							var source = PermissionsSettings.PresetPermissions[permission.PermissionName];
							ObservablePermissionsSourceList.Add(source);

							if(permission.Id > 0)
								deletePermissionList.Add(permission);

							OrderPermission();
						},
						(permission) => true
					);
				}
				return removePermissionCommand;
			}
		}

		public override DelegateCommand SaveCommand {
			get {
				if(saveCommand == null) {
					saveCommand = new DelegateCommand(
						() => {
							foreach(HierarchicalPresetPermissionBase item in ObservablePermissionsList)
								UoW.Save(item);

							foreach(var item in deletePermissionList)
								UoW.Delete(item);
						},
						() => true
					);
				}
				return saveCommand;
			}
		}
	}
}
