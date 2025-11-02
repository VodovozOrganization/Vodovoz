using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Permissions;

namespace Vodovoz.Domain.Permissions
{
	public class HierarchicalPresetPermissionBase : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual PresetPermissionType PresetPermissionType { get; set; }

		private string permissionName;
		[Display(Name = "Право")]
		public virtual string PermissionName {
			get => permissionName;
			set => SetField(ref permissionName, value, () => PermissionName);
		}

		private bool value;
		[Display(Name = "Значение права")]
		public virtual bool Value {
			get => value;
			set => SetField(ref this.value, value, () => Value);
		}

		public virtual PresetUserPermissionSource PermissionSource {
			get {
				if(!PermissionsSettings.PresetPermissions.ContainsKey(PermissionName)) {
					return null;
				}
				return PermissionsSettings.PresetPermissions[PermissionName];
			}
		}

		public virtual string DisplayName => PermissionSource != null ? PermissionSource.DisplayName : PermissionName;

		public virtual bool IsLostPermission => PermissionSource == null;
	}

	public enum PresetPermissionType
	{
		[Display(Name = "Для пользователя")]
		user,
		[Display(Name = "Для подразделения")]
		subdivision
	}
}
