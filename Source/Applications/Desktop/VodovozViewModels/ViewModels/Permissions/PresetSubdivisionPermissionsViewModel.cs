using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Permissions;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.ViewModels.Permissions
{
	public class PresetSubdivisionPermissionsViewModel : PresetPermissionsViewModelBase
	{
		protected Subdivision subdivision;

		public PresetSubdivisionPermissionsViewModel
		(
			IUnitOfWork unitOfWork, 
			IPermissionRepository permissionRepository,
			Subdivision subdivision,
			UsersPresetPermissionValuesGetter usersPresetPermissionValuesGetter,
			UserPermissionsExporter userPermissionsExporter
		): base(unitOfWork, permissionRepository, usersPresetPermissionValuesGetter, userPermissionsExporter)
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

		public override DelegateCommand AddPermissionCommand =>
			addPermissionCommand ?? (addPermissionCommand = new DelegateCommand(
				() =>
				{
					if(SelectedPresetUserPermissionSource is null)
					{
						return;
					}
					
					var newPermission = new HierarchicalPresetSubdivisionPermission
					{
						PermissionName = SelectedPresetUserPermissionSource.Name,
						Value = true,
						Subdivision = subdivision
					};
					ObservablePermissionsList.Add(newPermission);

					var sourceItem = ObservablePermissionsSourceList
						.SingleOrDefault(x => x.Name == newPermission.PermissionName);
					var deletedPermission =
						deletePermissionList.FirstOrDefault(x => x.PermissionName == SelectedPresetUserPermissionSource.Name);
					
					if(deletedPermission != null)
					{
						deletePermissionList.Remove(deletedPermission);
					}
					
					ObservablePermissionsSourceList.Remove(sourceItem);
					OrderPermission();
				}
			));

		public override DelegateCommand RemovePermissionCommand =>
			removePermissionCommand ?? (removePermissionCommand = new DelegateCommand(
				() =>
				{
					if(SelectedHierarchicalPresetPermissionBase is null)
					{
						return;
					}
					
					var source = PermissionsSettings.PresetPermissions[SelectedHierarchicalPresetPermissionBase.PermissionName];
					ObservablePermissionsSourceList.Add(source);

					if(SelectedHierarchicalPresetPermissionBase.Id > 0)
					{
						deletePermissionList.Add(SelectedHierarchicalPresetPermissionBase);
					}
					
					ObservablePermissionsList.Remove(SelectedHierarchicalPresetPermissionBase);
					OrderPermission();
				}
			));

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
