using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.Core.Domain.Users
{
	/// <summary>
	/// Роль пользователя при работе с базами данных
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Accusative = "роль пользователя при работе с БД",
		AccusativePlural = "роли пользователей при работе с БД",
		Genitive = "роли пользователя при работе с БД",
		GenitivePlural = "ролей пользователей при работе с БД",
		Nominative = "роль пользователя при работе с БД",
		NominativePlural = "роли пользователей при работе с БД",
		Prepositional = "роли пользователя при работе с БД",
		PrepositionalPlural = "ролях пользователей при работе с БД")]
	public class UserRole : PropertyChangedBase, IDomainObject
	{
		private string _name;
		private string _description;
		
		private IObservableList<User> _users = new ObservableList<User>();
		private IObservableList<AvailableDatabase> _availableDatabases = new ObservableList<AvailableDatabase>();
		private IObservableList<PrivilegeBase> _privileges = new ObservableList<PrivilegeBase>();
		
		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Название роли
		/// </summary>
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
		
		/// <summary>
		/// Описание
		/// </summary>
		[Display(Name = "Описание")]
		public virtual string Description
		{
			get => _description;
			set => SetField(ref _description, value);
		}

		/// <summary>
		/// Пользователи
		/// </summary>
		[Display(Name = "Пользователи")]
		public virtual IObservableList<User> Users
		{
			get => _users;
			set => SetField(ref _users, value);
		}

		/// <summary>
		/// Привилегии
		/// </summary>
		[Display(Name = "Привилегии")]
		public virtual IObservableList<PrivilegeBase> Privileges
		{
			get => _privileges;
			set => SetField(ref _privileges, value);
		}

		/// <summary>
		/// Доступные базы данных
		/// </summary>
		[Display(Name = "Доступные базы данных")]
		public virtual IObservableList<AvailableDatabase> AvailableDatabases
		{
			get => _availableDatabases;
			set => SetField(ref _availableDatabases, value);
		}

		/// <summary>
		/// Добавляет доступную базу данных, если она ещё не добавлена
		/// </summary>
		/// <param name="availableDatabase">Доступная база данных</param>
		public virtual void AddAvailableDatabase(AvailableDatabase availableDatabase)
		{
			if(!AvailableDatabases.Contains(availableDatabase))
			{
				AvailableDatabases.Add(availableDatabase);
			}
		}
	}
}
