using System;

namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Параметры запроса получения изменений документооборотов
	/// </summary>
	public class GetDocFlowsUpdatesParameters
	{
		/// <summary>
		/// Фильтр по статусу документооборота
		/// </summary>
		public string DocFlowStatus { get; set; }
		/// <summary>
		/// Дата, с которой будет идти выборка
		/// </summary>
		public long? LastEventTimeStamp { get; set; }
		/// <summary>
		/// Фильтр по направлению документооборота: Входящий и Исходящий
		/// </summary>
		public string DocFlowDirection { get; set; }
		/// <summary>
		/// Фильтр по подразделению
		/// </summary>
		public Guid? DepartmentId { get; set; }
		/// <summary>
		/// Включение/исключение информации по служебным документам из контейнера
		/// </summary>
		public bool IncludeTransportInfo { get; set; }
		/// <summary>
		/// Включение/исключение расширенных статусов
		/// </summary>
		public bool IncludeExtendedDocFlowStatuses { get; set; }
	}
}
