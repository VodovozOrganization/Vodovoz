using System;

namespace Mango.Employees.Library.Options
{
	/// <summary>
	/// Настройки воркера деактивации сотрудников Манго
	/// </summary>
	public class DriverMangoEmployeeDeactivationOptions
	{
		/// <summary>
		/// Интервал проверки условия запуска воркера
		/// </summary>
		public TimeSpan Interval { get; set; }

		/// <summary>
		/// Время суток (МСК), в которое запускается обработка
		/// </summary>
		public TimeSpan RunTime { get; set; }

		/// <summary>
		/// Минимальный добавочный номер пула. Через API Манго удаляются только номера из этого диапазона
		/// </summary>
		public int ExtensionNumberPoolStart { get; set; }

		/// <summary>
		/// Максимальный добавочный номер пула. Через API Манго удаляются только номера из этого диапазона
		/// </summary>
		public int ExtensionNumberPoolEnd { get; set; }
	}
}
