using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Security;

namespace Vodovoz.Domain.Employees
{
	public class User: QS.Project.Domain.UserBase
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
	}
}

