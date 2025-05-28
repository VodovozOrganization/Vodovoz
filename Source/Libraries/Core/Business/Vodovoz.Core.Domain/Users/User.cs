using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Vodovoz.Domain.Permissions;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Security;

namespace Vodovoz.Domain.Employees
{
	public class User: QS.Project.Domain.UserBase, IValidatableObject
	{
		private bool _needPasswordChange;
		private UserRole _currentUserRole;
		private IList<RegisteredRM> _registeredRMs = new List<RegisteredRM>();
		private IList<UserRole> _userRoles = new List<UserRole>();
		private GenericObservableList<UserRole> _observableUserRoles;

		public virtual string WarehouseAccess { get; set; }

		[Display(Name = "Требуется смена пароля")]
		public virtual bool NeedPasswordChange {
			get => _needPasswordChange;
			set => SetField(ref _needPasswordChange, value);
		}

		/// <summary>
		/// Пользователи операционной системы доступные для подключения по этой учетной записи пользователя
		/// </summary>
		[Display(Name = "Пользователи операционной системы доступные для подключения по этой учетной записи пользователя")]
		public virtual IList<RegisteredRM> RegisteredRMs
		{
			get => _registeredRMs;
			set => SetField(ref _registeredRMs, value);
		}
		
		[Display(Name = "Текущая роль пользователя")]
		public virtual UserRole CurrentUserRole
		{
			get => _currentUserRole;
			set => SetField(ref _currentUserRole, value);
		}
		
		[Display(Name = "Роли пользователя")]
		public virtual IList<UserRole> UserRoles
		{
			get => _userRoles;
			set => SetField(ref _userRoles, value);
		}
		
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<UserRole> ObservableUserRoles =>
			_observableUserRoles ?? (_observableUserRoles = new GenericObservableList<UserRole>(UserRoles));

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var regex = new Regex(@"^[A-Za-z\d.,_-]+\Z");
			if(!regex.IsMatch(Login))
			{
				yield return new ValidationResult("Логин может состоять только из букв английского алфавита, нижнего подчеркивания, дефиса, точки и запятой", new[] { nameof(Login) });
			}
		}

		public virtual IDictionary<WarehousePermissionsType, IList<int>> GetWarehousePermissions()
		{
			return JsonConvert.DeserializeObject<Dictionary<WarehousePermissionsType, IList<int>>>(WarehouseAccess);
		}
	}
}

