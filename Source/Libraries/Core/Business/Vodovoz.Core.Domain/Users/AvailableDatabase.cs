using System.Collections.Generic;
using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Users
{
	/// <summary>
	/// Доступные базы данных для пользовательской роли.
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Accusative = "доступную БД для пользовательской роли",
		AccusativePlural = "доступные БД для пользовательской роли",
		Genitive = "доступной БД для пользовательской роли",
		GenitivePlural = "доступных БД для пользовательской роли",
		Nominative = "доступная БД для пользовательской роли",
		NominativePlural = "доступные БД для пользовательской роли",
		Prepositional = "доступной БД для пользовательской роли",
		PrepositionalPlural = "доступных БД для пользовательской роли")]
	public class AvailableDatabase : IDomainObject
	{
		private int _id;
		private string _name;
		private IList<UserRole> _userRoles = new List<UserRole>();
		private IList<PrivilegeName> _unavailableForPrivilegeNames = new List<PrivilegeName>();

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id
		{
			get => _id;
			set => _id = value;
		}

		/// <summary>
		/// Имя базы данных
		/// </summary>
		public virtual string Name
		{
			get => _name;
			set => _name = value;
		}

		/// <summary>
		/// Роль пользователя, к которой относится данная база данных
		/// </summary>
		public virtual IList<UserRole> UserRoles
		{
			get => _userRoles;
			set => _userRoles = value;
		}

		/// <summary>
		/// Список привилегий, которые недоступны для данной базы данных
		/// </summary>
		public virtual IList<PrivilegeName> UnavailableForPrivilegeNames
		{
			get => _unavailableForPrivilegeNames;
			set => _unavailableForPrivilegeNames = value;
		}
	}
}
