using System;
using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	public abstract class TemplateDeliveryScheduleBaseDto
	{
		/// <summary>
		/// Тип доставки <see cref="CustomerTemplateDeliveryScheduleType"/>
		/// </summary>
		public CustomerTemplateDeliveryScheduleType Type { get; set; }
		
		/// <summary>
		/// День недели
		/// Доступно только при недельном типе доставки <see cref="ExternalCustomerTemplateDeliveryScheduleType/>
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public DayOfWeek? WeekDay { get; set; }
		
		/// <summary>
		/// Повторы через определенное количество недель
		/// Доступно только при недельном типе доставки <see cref="ExternalCustomerTemplateDeliveryScheduleType/> 
		/// 1 - каждую неделю
		/// 2 - раз в две недели
		/// 3 - раз в три недели и т.д.
		/// </summary>
		public int? RepeatEveryWeeks { get; set; }
		
		/// <summary>
		/// Дата первой доставки по автозаказу
		/// </summary>
		public DateTime? FirstDeliveryDate { get; set; }
		
		/// <summary>
		/// Число месяца
		/// Доступно только при месячном типе доставки <see cref="ExternalCustomerTemplateDeliveryScheduleType"/>
		/// </summary>
		public int? DayOfMonth { get; set; }
	}
}
