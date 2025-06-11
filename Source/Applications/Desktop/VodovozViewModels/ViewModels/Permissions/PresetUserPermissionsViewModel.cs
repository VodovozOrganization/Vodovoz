using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using FluentNHibernate.Conventions;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Permissions;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.ViewModels.Permissions
{
	public class PresetUserPermissionsViewModel : PresetPermissionsViewModelBase
	{
		private readonly User _user;

		public PresetUserPermissionsViewModel(
			IUnitOfWork unitOfWork,
			IPermissionRepository permissionRepository,
			User user,
			UsersPresetPermissionValuesGetter usersPresetPermissionValuesGetter,
			UserPermissionsExporter userPermissionsExporter)
			: base(unitOfWork, permissionRepository, usersPresetPermissionValuesGetter, userPermissionsExporter)
		{
			_user = user ?? throw new ArgumentNullException(nameof(user));
			ObservablePermissionsList = new GenericObservableList<HierarchicalPresetPermissionBase>();
			ObservablePermissionsSourceList = new GenericObservableList<PresetUserPermissionSource>();
			UpdateData();
		}

		public void UpdateData(IEnumerable<HierarchicalPresetPermissionBase> newUserPermissions = null)
		{
			permissionList = newUserPermissions == null
				? permissionRepository.GetAllPresetUserPermission(UoW, _user.Id).OfType<HierarchicalPresetPermissionBase>().ToList()
				: new List<HierarchicalPresetPermissionBase>(newUserPermissions);

			originalPermissionsSourceList = PermissionsSettings.PresetPermissions.Values.ToList();

			OrderPermission();
			FillObservableList(permissionList, ObservablePermissionsList);

			foreach(var item in permissionList)
			{
				var sourceItem = originalPermissionsSourceList.SingleOrDefault(x => x.Name == item.PermissionName);
				if(sourceItem != null)
				{
					originalPermissionsSourceList.Remove(sourceItem);
				}
			}

			FillObservableList(originalPermissionsSourceList, ObservablePermissionsSourceList);
		}

		public override void StartSearch(string searchString)
		{
			FillObservableList(permissionList, ObservablePermissionsList);
			FillObservableList(originalPermissionsSourceList, ObservablePermissionsSourceList);
			
			if(!searchString.IsEmpty())
			{
				for(int i = 0; i < ObservablePermissionsSourceList.Count; i++)
				{
					if(ObservablePermissionsSourceList[i].DisplayName.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) == -1)
					{
						ObservablePermissionsSourceList.Remove(ObservablePermissionsSourceList[i]);
						i--;
					}
				}
				for(int i = 0; i < ObservablePermissionsList.Count; i++)
				{
					if(ObservablePermissionsList[i].DisplayName.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) == -1)
					{
						ObservablePermissionsList.Remove(ObservablePermissionsList[i]);
						i--;
					}
				}
			}
		}

		public override DelegateCommand AddPermissionCommand =>
			addPermissionCommand ?? (addPermissionCommand = new DelegateCommand(
				() =>
				{
					if(SelectedPresetUserPermissionSource is null)
					{
						return;
					}
					
					var newPermission = new HierarchicalPresetUserPermission
					{
						PermissionName = SelectedPresetUserPermissionSource.Name,
						Value = true,
						User = _user
					};
					ObservablePermissionsList.Insert(0, newPermission);
					permissionList.Insert(0, newPermission);

					var sourceItem = ObservablePermissionsSourceList
						.SingleOrDefault(x => x.Name == newPermission.PermissionName);
					
					var deletedPermission =
						deletePermissionList.FirstOrDefault(x => x.PermissionName == SelectedPresetUserPermissionSource.Name);
					
					if(deletedPermission != null)
					{
						deletePermissionList.Remove(deletedPermission);
					}
					
					ObservablePermissionsSourceList.Remove(sourceItem);
					originalPermissionsSourceList.Remove(sourceItem);
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
					
					if(SelectedHierarchicalPresetPermissionBase.Id > 0)
					{
						deletePermissionList.Add(SelectedHierarchicalPresetPermissionBase);
					}
					
					ObservablePermissionsList.Remove(SelectedHierarchicalPresetPermissionBase);
					permissionList.Remove(SelectedHierarchicalPresetPermissionBase);

					ObservablePermissionsSourceList.Add(source);
					originalPermissionsSourceList.Add(source);
				}
			));

		public override DelegateCommand SaveCommand =>
			saveCommand ?? (saveCommand = new DelegateCommand(
				() =>
				{
					foreach(var item in permissionList)
					{
						UoW.Save(item);
					}
					foreach(var item in deletePermissionList)
					{
						UoW.Delete(item);
					}
				}
			));

		private void FillObservableList<T>(IList<T> sourceList, GenericObservableList<T> observableList)
		{
			observableList.Clear();
			
			foreach(var item in sourceList)
			{
				observableList.Add(item);
			}
		}
	}
}
