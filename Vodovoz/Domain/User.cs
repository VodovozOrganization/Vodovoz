using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubject("Пользователи")]
	public class User
	{
		#region Свойства
		public virtual int Id { get; set; }
		[Required(ErrorMessage = "Имя пользователя должно быть заполнено.")]
		public virtual string Name { get; set; }
		[Required(ErrorMessage = "Логин пользователя должен быть заполнен.")]
		public virtual string Login { get; set; }
		#endregion

		public User()
		{
			Name = String.Empty;
			Login = String.Empty;
		}
	}
}

