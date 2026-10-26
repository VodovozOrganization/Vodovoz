using System;
using CustomerOrders.Contracts.V8.Sale.Templates;
using Vodovoz.Core.Domain.Sale;

namespace CustomerOrdersApi.Library.V8.Extensions
{
	public static class OnlineOrderTemplateStatusExtensions
	{
		public static TemplateStatusDto ToApiTemplateStatus(this OnlineOrderTemplateStatus source)
		{
			switch(source)
			{
				case OnlineOrderTemplateStatus.AwaitingPayment:
					return TemplateStatusDto.AwaitingPayment;
				case OnlineOrderTemplateStatus.PaymentProblem:
					return TemplateStatusDto.PaymentProblem;
				case OnlineOrderTemplateStatus.Active:
					return TemplateStatusDto.Active;
				case OnlineOrderTemplateStatus.Inactive:
					return TemplateStatusDto.Inactive;
				case OnlineOrderTemplateStatus.Archived:
					return TemplateStatusDto.Archived;
				default:
					throw new IndexOutOfRangeException(
						$"Неизвестный статус шаблона автозаказа {source.ToString()}. Проверьте настройку расширения ${nameof(ToApiTemplateStatus)}");
			}
		}
	}
}
