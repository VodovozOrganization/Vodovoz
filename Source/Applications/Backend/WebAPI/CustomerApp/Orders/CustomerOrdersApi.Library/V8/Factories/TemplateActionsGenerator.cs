using System;
using CustomerOrders.Contracts.V8.Sale.Templates;

namespace CustomerOrdersApi.Library.V8.Factories
{
	public class TemplateActionsGenerator : ITemplateActionsGenerator
	{
		public OrderTemplateActionsDto Generate(TemplateStatusDto status)
		{
			switch(status)
			{
				case TemplateStatusDto.Archived:
					return new OrderTemplateActionsDto(false, false, false);
				case TemplateStatusDto.Paused:
					return new OrderTemplateActionsDto(false, true, true);
				case TemplateStatusDto.Active:
					return new OrderTemplateActionsDto(true, false, true);
				case TemplateStatusDto.AwaitingPayment:
				case TemplateStatusDto.ManualProcessing:
				case TemplateStatusDto.PaymentProblem:
					//TODO-6121 что делаем в этих случаях?
					return new OrderTemplateActionsDto(false, false, true);
				default:
					throw new ArgumentOutOfRangeException(
						nameof(status),
						status,
						"Пришло неизвестное значение статуса шаблона автозаказа. Невозможно создать TemplateActionsDto");
			}
		}
	}
}
