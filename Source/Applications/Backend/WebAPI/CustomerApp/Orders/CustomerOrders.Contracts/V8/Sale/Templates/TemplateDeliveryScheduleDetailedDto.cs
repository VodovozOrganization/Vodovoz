using CustomerApp.Contracts.Sale.Templates;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	public class TemplateDeliveryScheduleDetailedDto : TemplateDeliveryScheduleBaseDto
	{
		/// <summary>
		/// Информация по интервалу доставки
		/// </summary>
		public DeliveryScheduleDto DeliveryInterval { get; set; }
		
		/// <summary>
		/// Описание доставки
		/// </summary>
		public string Description { get; set; }

		public static TemplateDeliveryScheduleDetailedDto Create(
			TemplateDeliveryScheduleBaseDto baseInfo,
			DeliveryScheduleDto interval,
			string description)
		{
			return new TemplateDeliveryScheduleDetailedDto
			{
				DayOfMonth = baseInfo.DayOfMonth,
				WeekDay = baseInfo.WeekDay,
				Type = baseInfo.Type,
				FirstDeliveryDate = baseInfo.FirstDeliveryDate,
				RepeatEveryWeeks = baseInfo.RepeatEveryWeeks,
				DeliveryInterval = interval,
				Description = description
			};
		}
	}
}
