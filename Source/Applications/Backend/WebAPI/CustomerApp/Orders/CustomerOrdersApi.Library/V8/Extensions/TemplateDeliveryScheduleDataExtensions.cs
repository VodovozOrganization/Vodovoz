using CustomerOrders.Contracts.V8.Sale.Templates;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Sale;

namespace CustomerOrdersApi.Library.V8.Extensions
{
	public static class TemplateDeliveryScheduleDataExtensions
	{
		public static TemplateDeliveryScheduleDto ToTemplateDeliveryScheduleDto(this TemplateDeliveryScheduleData source)
		{
			return new TemplateDeliveryScheduleDto
			{
				WeekDay = source.Weekday.ToDayOfWeek(),
				RepeatEveryWeeks = source.RepeatEveryWeeks,
				DayOfMonth = source.DayOfMonth,
				FirstDeliveryDate = source.FirstDeliveryDate,
				Type = source.ScheduleType.ToCustomerTemplateDeliveryScheduleType(),
				DeliveryScheduleId = source.DeliveryScheduleId,
			};
		}
	}
}
