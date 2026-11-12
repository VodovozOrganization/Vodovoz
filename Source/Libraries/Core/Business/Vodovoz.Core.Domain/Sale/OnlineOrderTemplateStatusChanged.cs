using System;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Sale
{
	/// <summary>
	/// Данные по смене статуса шаблона
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Смены статусов шаблонов автозаказов",
		Nominative = "Смена статуса шаблона автозаказа",
		Prepositional = "Смене статуса шаблона автозаказа",
		PrepositionalPlural = "Сменах статусов шаблонов автозаказов"
	)]
	[HistoryTrace]
	public class OnlineOrderTemplateStatusChanged : IDomainObject
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }
		
		/// <summary>
		/// Идентификатор шаблона
		/// </summary>
		public virtual int TemplateId { get; set; }

		/// <summary>
		/// Дата и время события смены статуса
		/// </summary>
		public virtual DateTime CreatedOn { get; set; }

		/// <summary>
		/// Идентификатор сотрудника
		/// Заполняется, если инициатор сотрудник
		/// </summary>
		public virtual int? EmployeeId { get; set; }

		/// <summary>
		/// Идентификатор клиента
		/// Заполняется, если инициатор клиент
		/// </summary>
		public virtual int? CounterpartyId { get; set; }

		/// <summary>
		/// Тип смены статуса
		/// </summary>
		public virtual OnlineOrderTemplateStatusChangedType Type { get; set; }

		public virtual void Update(int? counterpartyId, int? employeeId)
		{
			CounterpartyId = counterpartyId;
			EmployeeId = employeeId;
			CreatedOn = DateTime.Now;
		}

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
