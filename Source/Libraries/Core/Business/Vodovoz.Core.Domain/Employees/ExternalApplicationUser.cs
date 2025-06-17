using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Employees
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Accusative = "пользователя внешнего приложения",
		AccusativePlural = "пользователей внешних приложений",
		Genitive = "пользователя внешнего приложения",
		GenitivePlural = "пользователей внешних приложений",
		Nominative = "пользователь внешнего приложения",
		NominativePlural = "пользователи внешних приложений",
		Prepositional = "пользователе внешнего приложения",
		PrepositionalPlural = "пользователях внешних приложений")]
	[HistoryTrace]
	public class ExternalApplicationUser : PropertyChangedBase, IDomainObject
	{
		private string _login;
		private string _password;
		private string _androidSessionKey;
		private string _token;
		private int? _employeeId;
		
		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Логин для приложения<br/>
		/// Имя пользователя
		/// </summary>
		[Display(Name = "Логин для приложения")]
		public virtual string Login
		{
			get => _login;
			set => SetField(ref _login, value);
		}

		/// <summary>
		/// Пароль для приложения
		/// </summary>
		[Display(Name = "Пароль для приложения")]
		public virtual string Password
		{
			get => _password;
			set => SetField(ref _password, value);
		}

		/// <summary>
		/// Ключ сессии для приложения
		/// </summary>
		[Display(Name = "Ключ сессии для приложения")]
		public virtual string SessionKey
		{
			get => _androidSessionKey;
			set => SetField(ref _androidSessionKey, value);
		}

		/// <summary>
		/// Токен приложения пользователя для отправки Push-сообщений
		/// </summary>
		[Display(Name = "Токен приложения пользователя для отправки Push-сообщений")]
		public virtual string Token
		{
			get => _token;
			set => SetField(ref _token, value);
		}

		/// <summary>
		/// Сотрудник
		/// </summary>
		[Display(Name = "Сотрудник")]
		[HistoryIdentifier(TargetType = typeof(EmployeeEntity))]
		public virtual int? EmployeeId
		{
			get => _employeeId;
			set => SetField(ref _employeeId, value);
		}

		public virtual ExternalApplicationType ExternalApplicationType { get; set; }
	}
}
