using System;
using CustomerOrders.Contracts.V8.Sale.Templates;
using Vodovoz.Core.Domain.Sale;

namespace CustomerOrdersApi.Library.V8.Extensions
{
	public static class CustomerTemplateDeliveryScheduleTypeExtensions
	{
		public static TemplateDeliveryScheduleType ToTemplateScheduleType(this CustomerTemplateDeliveryScheduleType source)
		{
			switch(source)
			{
				case CustomerTemplateDeliveryScheduleType.Weekly:
					return TemplateDeliveryScheduleType.Weekly;
				case CustomerTemplateDeliveryScheduleType.Monthly:
					return TemplateDeliveryScheduleType.Monthly;
				default:
					throw new ArgumentOutOfRangeException(
						nameof(CustomerTemplateDeliveryScheduleType),
						$"Пришел неизвестный тип периодичности автозаказа {source}");
			}
		}
	}
}
