using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Users
{
	/// <summary>
	/// Зарегистрированный пользователь операционной системы, который может подключаться к программе
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Accusative = "зарегистрированный RM",
		AccusativePlural = "зарегистрированные RM",
		Genitive = "зарегистрированного RM",
		GenitivePlural = "зарегистрированных RM",
		Nominative = "зарегистрированный RM",
		NominativePlural = "зарегистрированные RM",
		Prepositional = "зарегистрированном RM",
		PrepositionalPlural = "зарегистрированных RM")]
	[EntityPermission]
	public class RegisteredRM : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _username;
		private string _domain;
		private string _sid;
		private IObservableList<User> _users = new ObservableList<User>();
		private bool _isActive;
		private int _id;

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Имя пользователя в системе
		/// </summary>
		[Display(Name = "Имя пользователя в системе")]
		public virtual string Username
		{
			get => _username;
			set => SetField(ref _username, value);
		}

		/// <summary>
		/// Имя домена
		/// </summary>
		[Display(Name = "Имя домена")]
		public virtual string Domain
		{
			get => _domain;
			set => SetField(ref _domain, value);
		}

		/// <summary>
		/// SID пользователя
		/// </summary>
		[Display(Name = "SID пользователя")]
		public virtual string SID
		{
			get => _sid;
			set => SetField(ref _sid, value);
		}

		/// <summary>
		/// Пользователи программы
		/// </summary>
		[Display(Name = "Пользователи программы доступные для подключения")]
		public virtual IObservableList<User> Users
		{
			get => _users;
			set => SetField(ref _users, value);
		}

		/// <summary>
		/// Запись активна
		/// </summary>
		[Display(Name = "Запись активна")]
		public virtual bool IsActive
		{
			get => _isActive;
			set => SetField(ref _isActive, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (string.IsNullOrWhiteSpace(Username))
			{
				yield return new ValidationResult("Имя пользователя не может быть пустым",
					new[] { nameof(Username) });
			}

			if (string.IsNullOrWhiteSpace(Domain))
			{
				yield return new ValidationResult("Домен не может быть пустым",
					new[] { nameof(Domain) });
			}

			if (string.IsNullOrWhiteSpace(SID))
			{
				yield return new ValidationResult("SID пользователя не может быть пустым",
					new[] { nameof(SID) });
			}
		}
	}
}
