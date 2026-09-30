using System;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Состояние заявки ЭДО, достаточное для проверки условий отклонений
	/// сервисом мониторинга документооборота
	/// </summary>
	public class EdoRequestMonitoringNode
	{
		/// <summary>
		/// Код заявки
		/// </summary>
		public int RequestId { get; set; }

		/// <summary>
		/// Время создания заявки
		/// </summary>
		public DateTime RequestTime { get; set; }

		/// <summary>
		/// Признак того, что по заявке создана задача ЭДО.
		/// Заполняется при выборке по кодам заявок; при выборке заявок без задач всегда ложь
		/// </summary>
		public bool HasTask { get; set; }
	}
}
