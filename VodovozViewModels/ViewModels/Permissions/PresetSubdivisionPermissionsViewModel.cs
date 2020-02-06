using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Permissions;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.ViewModels.Permissions
{
	public class PresetSubdivisionPermissionsViewModel: PresetPermissionsViewModelBase
	{
		protected Subdivision subdivision;

		public PresetSubdivisionPermissionsViewModel
		(
			IUnitOfWork unitOfWork, 
			IPermissionRepository permissionRepository, 
			Subdivision subdivision
		): base(unitOfWork, permissionRepository)
		{
			this.subdivision = subdivision ?? throw new ArgumentNullException(nameof(subdivision));

			permissionList = permissionRepository.GetAllPresetSubdivisionPermission(UoW, subdivision)
													.OfType<HierarchicalPresetPermissionBase>().ToList();
			ObservablePermissionsList = new GenericObservableList<HierarchicalPresetPermissionBase>(permissionList.ToList());

			originalPermissionsSourceList = PermissionsSettings.PresetPermissions.Values.ToList();
			foreach(var item in permissionList) {
				var sourceItem = originalPermissionsSourceList.SingleOrDefault(x => x.Name == item.PermissionName);
				if(sourceItem != null)
					originalPermissionsSourceList.Remove(sourceItem);
			}
			ObservablePermissionsSourceList = new GenericObservableList<PresetUserPermissionSource>(originalPermissionsSourceList);

			OrderPermission();
		}

		public override DelegateCommand<PresetUserPermissionSource> AddPermissionCommand {
			get {
				if(addPermissionCommand == null) {
					addPermissionCommand = new DelegateCommand<PresetUserPermissionSource>(
						(source) => {
							var newPermission = new HierarchicalPresetSubdivisionPermission {
								PermissionName = source.Name,
								Value = true,
								Subdivision = subdivision
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
							var permission = permissionBase as HierarchicalPresetSubdivisionPermission;
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
								UoW.Save(item as HierarchicalPresetSubdivisionPermission);

							foreach(var item in deletePermissionList)
								UoW.Delete(item as HierarchicalPresetSubdivisionPermission);
						},
						() => true
					);
				}
				return saveCommand;
			}
		}
	}
}
