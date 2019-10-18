using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Project.Domain;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Permissions
{
	public class EntityPermissionExtended : PropertyChangedBase, IDomainObject
	{
		public EntityPermissionExtended(){}

		public virtual int Id { get; set; }

		private bool? isPermissionAvailable;
		[Display(Name = "Доступно ли право ?")]
		public virtual bool? IsPermissionAvailable {
			get => isPermissionAvailable;
			set => SetField(ref isPermissionAvailable, value);
		}

		private string permissionId;
		public virtual string PermissionId {
			get => permissionId;
			set => SetField(ref permissionId, value);
		}

		private TypeOfEntity typeOfEntity;
		[Display(Name = "Тип сущности")]
		public virtual TypeOfEntity TypeOfEntity {
			get => typeOfEntity;
			set => SetField(ref typeOfEntity, value, () => TypeOfEntity);
		}

		private User user;
		[Display(Name = "Пользователь")]
		public virtual User User {
			get => user;
			set => SetField(ref user, value);
		}

		private Subdivision subdivision;
		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value);
		}

	}
}
