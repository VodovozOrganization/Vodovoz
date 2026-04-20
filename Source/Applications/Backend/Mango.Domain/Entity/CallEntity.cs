using System;
using Mango.Domain.Enums;

namespace Mango.Domain.Entity
{
	/// <summary>
	/// Звонок
	/// </summary>
	public class CallEntity
	{
		/// <summary>
		/// ID из mango
		/// </summary>
		public string EntryId { get; set; } = string.Empty;
		
		/// <summary>
		/// Название группы
		/// </summary>
		public string GroupName { get; set; }
		
		/// <summary>
		/// Хеш для идентификации звонка
		/// </summary>
		public string UnicHash { get; set; }

		/// <summary>
		/// Время начала звонка
		/// </summary>
		public DateTime StartTime { get; set; }

		/// <summary>
		/// Время конца звонка
		/// </summary>
		public DateTime EndTime { get; set; }

		/// <summary>
		/// Время ответа
		/// </summary>
		public DateTime? AnswerTime { get; set; }

		/// <summary>
		/// Направление звонка
		/// </summary>
		public CallDirect CallDirect { get; set; }

		/// <summary>
		/// Пропущен ли звонок
		/// </summary>
		public bool IsMissed { get; set; }
	}
}
