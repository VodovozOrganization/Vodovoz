using System;
using System.Collections.Generic;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	public class OnlineOrderTemplateCardForListData
	{
		/// <summary>
		/// Идентификатор шаблона
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Активен шаблон
		/// </summary>
		public bool IsActive { get; set; }
		/// <summary>
		/// Идентификатор контрагента
		/// </summary>
		public int CounterpartyId { get; set; }
		/// <summary>
		/// Адрес доставки
		/// </summary>
		public string DeliveryAddress { get; set; }
		/// <summary>
		/// Интервал доставки в формате с 07:00 до 16:00
		/// </summary>
		public string DeliverySchedule { get; set; }
		/// <summary>
		/// Интервал повторов
		/// </summary>
		public string RepeatOrder { get; set; }
		/// <summary>
		/// Дата следующей доставки
		/// </summary>
		public DateTime NextDeliveryDate { get; set; }
	}
}
