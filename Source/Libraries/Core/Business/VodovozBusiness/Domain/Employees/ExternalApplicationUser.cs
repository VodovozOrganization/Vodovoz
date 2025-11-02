using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "пользователи внешних приложений",
		Nominative = "пользователь внешнего приложения")]
	[HistoryTrace]
	public class ExternalApplicationUser : PropertyChangedBase, IDomainObject
	{
		private string _login;
		private string _password;
		private string _androidSessionKey;
		private string _token;
		private Employee _employee;
		
		public virtual int Id { get; set; }

		[Display(Name = "Логин для приложения")]
		public virtual string Login
		{
			get => _login;
			set => SetField(ref _login, value);
		}

		[Display(Name = "Пароль для приложения")]
		public virtual string Password
		{
			get => _password;
			set => SetField(ref _password, value);
		}

		[Display(Name = "Ключ сессии для приложения")]
		public virtual string SessionKey
		{
			get => _androidSessionKey;
			set => SetField(ref _androidSessionKey, value);
		}

		[Display(Name = "Токен приложения пользователя для отправки Push-сообщений")]
		public virtual string Token
		{
			get => _token;
			set => SetField(ref _token, value);
		}

		[Display(Name = "Сотрудник")]
		public virtual Employee Employee
		{
			get => _employee;
			set => SetField(ref _employee, value);
		}

		public virtual ExternalApplicationType ExternalApplicationType { get; set; }
	}
}
