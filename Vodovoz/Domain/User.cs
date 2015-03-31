using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubject ("Пользователи")]
	public class User: PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Required (ErrorMessage = "Имя пользователя должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string login;

		[Required (ErrorMessage = "Логин пользователя должен быть заполнен.")]
		public virtual string Login {
			get { return login; }
			set { SetField (ref login, value, () => Login); }
		}

		#endregion

		public User ()
		{
			Name = String.Empty;
			Login = String.Empty;
		}
	}
}

