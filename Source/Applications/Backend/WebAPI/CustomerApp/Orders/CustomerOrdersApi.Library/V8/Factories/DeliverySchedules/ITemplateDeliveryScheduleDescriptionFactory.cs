using System;
using CustomerOrders.Contracts.V8.Sale.Templates;

namespace CustomerOrdersApi.Library.V8.Factories.DeliverySchedules
{
	public interface ITemplateDeliveryScheduleDescriptionFactory
	{
		string Create(
			TemplateDeliveryScheduleBaseDto deliverySchedule,
			TimeSpan fromInterval,
			TimeSpan toInterval);
	}
}
