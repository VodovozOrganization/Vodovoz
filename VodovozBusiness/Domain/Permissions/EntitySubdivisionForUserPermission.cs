using System;
using System.ComponentModel.DataAnnotations;
using QS.Project.Domain;

namespace Vodovoz.Domain.Permissions
{
	public class EntitySubdivisionForUserPermission : EntitySubdivisionPermission
	{
		private UserBase user;
		[Display(Name = "Пользователь")]
		public virtual UserBase User {
			get => user;
			set => SetField(ref user, value, () => User);
		}
	}
}
