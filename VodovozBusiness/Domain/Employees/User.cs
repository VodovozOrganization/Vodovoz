using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Vodovoz.Domain.Permissions.Warehouse;
using Vodovoz.Domain.Security;

namespace Vodovoz.Domain.Employees
{
	public class User: QS.Project.Domain.UserBase, IValidatableObject
	{
		public virtual string WarehouseAccess { get; set; }

		[Display(Name = "Требуется смена пароля")]
		bool needPasswordChange;
		public virtual bool NeedPasswordChange {
			get => needPasswordChange;
			set => SetField(ref needPasswordChange, value);
		}

		private IList<RegisteredRM> registeredRMs = new List<RegisteredRM>();
		/// <summary>
		/// Пользователи операционной системы доступные для подключения по этой учетной записи пользователя
		/// </summary>
		[Display(Name = "Пользователи операционной системы доступные для подключения по этой учетной записи пользователя")]
		public virtual IList<RegisteredRM> RegisteredRMs
		{
			get => registeredRMs;
			set => SetField(ref registeredRMs, value);
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

