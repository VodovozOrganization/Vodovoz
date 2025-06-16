using QS.Extensions.Observable.Collections.List;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.Core.Domain.Users
{
	/// <summary>
	/// Пользователь системы.
	/// </summary>
	public class User : QS.Project.Domain.UserBase, IValidatableObject
	{
		private bool _needPasswordChange;
		private UserRole _currentUserRole;
		private IObservableList<RegisteredRM> _registeredRMs = new ObservableList<RegisteredRM>();
		private IObservableList<UserRole> _userRoles = new ObservableList<UserRole>();
		private string _warehouseAccess;

		/// <summary>
		/// Права доступа к складам пользователя в виде JSON-строки.
		/// </summary>
		public virtual string WarehouseAccess
		{
			get => _warehouseAccess;
			set => _warehouseAccess = value;
		}

		/// <summary>
		/// Требуется ли смена пароля при следующем входе в систему
		/// </summary>
		[Display(Name = "Требуется смена пароля")]
		public virtual bool NeedPasswordChange
		{
			get => _needPasswordChange;
			set => SetField(ref _needPasswordChange, value);
		}

		/// <summary>
		/// Пользователи операционной системы доступные для подключения по этой учетной записи пользователя
		/// </summary>
		[Display(Name = "Пользователи операционной системы доступные для подключения по этой учетной записи пользователя")]
		public virtual IObservableList<RegisteredRM> RegisteredRMs
		{
			get => _registeredRMs;
			set => SetField(ref _registeredRMs, value);
		}

		/// <summary>
		/// Текущая роль пользователя в системе
		/// </summary>
		[Display(Name = "Текущая роль пользователя")]
		public virtual UserRole CurrentUserRole
		{
			get => _currentUserRole;
			set => SetField(ref _currentUserRole, value);
		}

		/// <summary>
		/// Роли пользователя в системе
		/// </summary>
		[Display(Name = "Роли пользователя")]
		public virtual IObservableList<UserRole> UserRoles
		{
			get => _userRoles;
			set => SetField(ref _userRoles, value);
		}

		/// <summary>
		/// Получает права доступа к складам пользователя в виде словаря,
		/// где ключ - тип прав доступа, значение - коллекция идентификаторов складов
		/// </summary>
		/// <returns></returns>
		public virtual IDictionary<WarehousePermissionsType, IEnumerable<int>> GetWarehousePermissions()
		{
			return JsonSerializer.Deserialize<Dictionary<WarehousePermissionsType, IEnumerable<int>>>(WarehouseAccess);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var regex = new Regex(@"^[A-Za-z\d.,_-]+\Z");
			if(!regex.IsMatch(Login))
			{
				yield return new ValidationResult("Логин может состоять только из букв английского алфавита, нижнего подчеркивания, дефиса, точки и запятой", new[] { nameof(Login) });
			}
		}
	}
}
