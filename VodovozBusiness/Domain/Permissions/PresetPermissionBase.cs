using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Permissions
{
	public class PresetPermissionBase : PropertyChangedBase, IDomainObject
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
	}

	public enum PresetPermissionType
	{
		user,
		subdivision
	}

	public class PresetPermissionTypeCustomType : NHibernate.Type.EnumStringType
	{
		public PresetPermissionTypeCustomType() : base(typeof(PresetPermissionType))
		{
		}
	}
}
