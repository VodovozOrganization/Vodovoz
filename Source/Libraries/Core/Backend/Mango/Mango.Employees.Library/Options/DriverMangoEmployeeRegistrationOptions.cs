using System;

namespace Mango.Employees.Library.Options
{
	/// <summary>
	/// Настройки воркера регистрации водителей как сотрудников Манго
	/// </summary>
	public class DriverMangoEmployeeRegistrationOptions
	{
		/// <summary>
		/// Интервал работы воркера
		/// </summary>
		public TimeSpan Interval { get; set; }

		/// <summary>
		/// Id роли, назначаемой создаваемому сотруднику ВАТС (access_role_id)
		/// </summary>
		public string AccessRoleId { get; set; }

		/// <summary>
		/// Id исходящей линии создаваемого сотрудника ВАТС (line_id)
		/// </summary>
		public string LineId { get; set; }

		/// <summary>
		/// Id группы ВАТС, в которую добавляются водители (group_id)
		/// </summary>
		public long DriversGroupId { get; set; }

		/// <summary>
		/// Минимальный добавочный номер пула
		/// </summary>
		public int ExtensionNumberPoolStart { get; set; }

		/// <summary>
		/// Максимальный добавочный номер пула
		/// </summary>
		public int ExtensionNumberPoolEnd { get; set; }

		/// <summary>
		/// Номер, на который переадресуется звонок, если водитель не ответил
		/// Добавляется вторым средством дозвона в карточку сотрудника ВАТС
		/// </summary>
		public string CallForwardingPhoneNumber { get; set; }

		/// <summary>
		/// Время ожидания ответа на номере водителя в секундах
		/// </summary>
		public int PhoneNumberWaitSeconds { get; set; }

		/// <summary>
		/// Время ожидания ответа на номере переадресации в секундах
		/// </summary>
		public int CallForwardingPhoneNumberWaitSeconds { get; set; }
	}
}
