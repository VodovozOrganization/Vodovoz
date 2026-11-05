using System;

namespace Vodovoz.Core.Domain.Sale
{
	/// <summary>
	/// Данные по смене статуса шаблона
	/// </summary>
	public class OnlineOrderTemplateStatusChanged
	{
		/// <summary>
		/// Идентификатор шаблона
		/// </summary>
		public int TemplateId { get; set; }

		/// <summary>
		/// Дата и время события смены статуса
		/// </summary>
		public DateTime CreatedOn { get; set; }

		/// <summary>
		/// Идентификатор сотрудника
		/// Заполняется, если инициатор сотрудник
		/// </summary>
		public int? EmployeeId { get; set; }

		/// <summary>
		/// Идентификатор клиента
		/// Заполняется, если инициатор клиент
		/// </summary>
		public int? CounterpartyId { get; set; }

		/// <summary>
		/// Тип смены статуса
		/// </summary>
		public OnlineOrderTemplateStatusChangedType Type { get; set; }

		public static OnlineOrderTemplateStatusChanged Create(
			int templateId,
			DateTime createdOn,
			int? employeeId,
			int? counterpartyId,
			OnlineOrderTemplateStatusChangedType type)
		{
			return new OnlineOrderTemplateStatusChanged
			{
				TemplateId = templateId,
				CreatedOn = createdOn,
				EmployeeId = employeeId,
				CounterpartyId = counterpartyId,
				Type = type
			};
		}
	}
	
	public enum OnlineOrderTemplateStatusChangedType
	{
		/// <summary>
		/// Приостановка шаблона
		/// </summary>
		ToPause,

		/// <summary>
		/// Возобновление шаблона
		/// </summary>
		ToResume,

		/// <summary>
		/// Архивирование шаблона
		/// </summary>
		ToArchive
	}
}
