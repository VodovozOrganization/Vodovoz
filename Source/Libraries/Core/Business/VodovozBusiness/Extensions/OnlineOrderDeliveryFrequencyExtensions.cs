using System;
using CustomerOrders.Contracts.V5.Orders.Templates;
using Vodovoz.Core.Domain.Orders.OnlineOrders;

namespace VodovozBusiness.Extensions
{
	public static class OnlineOrderDeliveryFrequencyExtensions
	{
		public static ExternalOnlineOrderDeliveryFrequency ToExternalOnlineOrderDeliveryFrequency(this OnlineOrderDeliveryFrequency source)
		{
			switch(source)
			{
				case OnlineOrderDeliveryFrequency.OnePerWeek:
					return ExternalOnlineOrderDeliveryFrequency.OnePerWeek;
				case OnlineOrderDeliveryFrequency.OneEveryTwoWeeks:
					return ExternalOnlineOrderDeliveryFrequency.OneEveryTwoWeeks;
				case OnlineOrderDeliveryFrequency.OneEveryThreeWeeks:
					return ExternalOnlineOrderDeliveryFrequency.OneEveryThreeWeeks;
				case OnlineOrderDeliveryFrequency.OneEveryFourWeeks:
					return ExternalOnlineOrderDeliveryFrequency.OneEveryFourWeeks;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение периодичности доставки");
			}
		}
		
		public static OnlineOrderDeliveryFrequency ToOnlineOrderDeliveryFrequency(this ExternalOnlineOrderDeliveryFrequency source)
		{
			switch(source)
			{
				case ExternalOnlineOrderDeliveryFrequency.OnePerWeek:
					return OnlineOrderDeliveryFrequency.OnePerWeek;
				case ExternalOnlineOrderDeliveryFrequency.OneEveryTwoWeeks:
					return OnlineOrderDeliveryFrequency.OneEveryTwoWeeks;
				case ExternalOnlineOrderDeliveryFrequency.OneEveryThreeWeeks:
					return OnlineOrderDeliveryFrequency.OneEveryThreeWeeks;
				case ExternalOnlineOrderDeliveryFrequency.OneEveryFourWeeks:
					return OnlineOrderDeliveryFrequency.OneEveryFourWeeks;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение периодичности доставки из ИПЗ");
			}
		}
	}
}
