using System;
using System.Collections.ObjectModel;

namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Изменения по отправленным документам
	/// </summary>
	public class EdoDocFlowUpdates
	{
		/// <summary>
		/// Информация о документооборотах
		/// </summary>
		public ReadOnlyCollection<EdoDocFlow> Updates { get; set; }
		/// <summary>
		/// Время запроса
		/// </summary>
		public DateTime RequestDateTime { get; set; }
		/// <summary>
		/// Время последнего события в списке
		/// </summary>
		public long? LastEventTimeStamp { get; set; }
		/// <summary>
		/// Флаг указания, что на сервере нет больше изменений
		/// </summary>
		public bool IsLast { get; set; }
	}
}
