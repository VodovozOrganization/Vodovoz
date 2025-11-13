using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Permissions;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;
using VodovozBusiness.Services.Subdivisions;

namespace Vodovoz.ViewModels.Permissions
{
	public class PresetSubdivisionPermissionsViewModel : PresetPermissionsViewModelBase
	{
		protected Subdivision _subdivision;

		public PresetSubdivisionPermissionsViewModel
		(
			IUnitOfWork unitOfWork, 
			IPermissionRepository permissionRepository,
			Subdivision subdivision,
			UsersPresetPermissionValuesGetter usersPresetPermissionValuesGetter,
			UserPermissionsExporter userPermissionsExporter
		): base(unitOfWork, permissionRepository, usersPresetPermissionValuesGetter, userPermissionsExporter)
		{
			_subdivision = subdivision ?? throw new ArgumentNullException(nameof(subdivision));

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
						Subdivision = _subdivision
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

		public void AddPermissionsFromSubdivision(
			ISubdivisionPermissionsService subdivisionPermissionsService,
			Subdivision sourceSubdivision)
		{
			var newPermissions = subdivisionPermissionsService.AddPresetPermissions(
				UoW,
				_subdivision,
				sourceSubdivision);

			AddOrReplacePermissions(newPermissions);
		}

		public void ReplacePermissionsFromSubdivision(
			ISubdivisionPermissionsService subdivisionPermissionsService,
			Subdivision sourceSubdivision)
		{
			var newPermissions = subdivisionPermissionsService.ReplacePresetPermissions(
				UoW,
				_subdivision,
				sourceSubdivision);

			RemoveAllAddedPermissions();
			AddOrReplacePermissions(newPermissions);
		}

		private void AddOrReplacePermissions(IEnumerable<HierarchicalPresetSubdivisionPermission> permissions)
		{
			foreach(var permission in permissions)
			{
				var addedPermission =
					ObservablePermissionsList
					.FirstOrDefault(x => x.PermissionName == permission.PermissionName);

				if(addedPermission != null && addedPermission.Value == permission.Value)
				{
					continue;
				}

				RemovePermissionIfAdded(permission.PermissionName);

				ObservablePermissionsList.Add(permission);

				var sourceItem = ObservablePermissionsSourceList
					.SingleOrDefault(x => x.Name == permission.PermissionName);

				ObservablePermissionsSourceList.Remove(sourceItem);
			}

			OrderPermission();
		}

		private void RemoveAllAddedPermissions()
		{
			while(ObservablePermissionsList.Any())
			{
				var permission = ObservablePermissionsList.ElementAt(0);

				RemovePermissionIfAdded(permission.PermissionName);
			}
		}

		private void RemovePermissionIfAdded(string permissionName)
		{
			var permission =
				ObservablePermissionsList
				.FirstOrDefault(x => x.PermissionName == permissionName);

			if(permission is null)
			{
				return;
			}

			var source = PermissionsSettings.PresetPermissions[permissionName];
			ObservablePermissionsSourceList.Add(source);

			deletePermissionList.Add(permission);

			ObservablePermissionsList.Remove(permission);
		}
	}
}
