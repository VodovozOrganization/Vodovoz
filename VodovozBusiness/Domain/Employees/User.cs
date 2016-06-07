using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Employees
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "пользователи",
		Nominative = "пользователь")]
	public class User: PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Имя пользователя")]
		[Required (ErrorMessage = "Имя пользователя должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string login;

		[Display (Name = "Логин")]
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

