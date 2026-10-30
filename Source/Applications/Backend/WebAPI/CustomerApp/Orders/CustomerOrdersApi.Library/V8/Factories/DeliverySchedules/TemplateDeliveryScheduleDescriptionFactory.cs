using System;
using CustomerOrders.Contracts.V8.Sale.Templates;
using CustomerOrdersApi.Library.V8.Extensions;
using CustomerOrdersApi.Library.V8.Services.Converters;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Sale;

namespace CustomerOrdersApi.Library.V8.Factories.DeliverySchedules
{
	public class TemplateDeliveryScheduleDescriptionFactory : ITemplateDeliveryScheduleDescriptionFactory
	{
		private readonly IRepeatEveryWeeksConverter _repeatEveryWeeksConverter;

		public TemplateDeliveryScheduleDescriptionFactory(
			IRepeatEveryWeeksConverter repeatEveryWeeksConverter
			)
		{
			_repeatEveryWeeksConverter = repeatEveryWeeksConverter ?? throw new ArgumentNullException(nameof(repeatEveryWeeksConverter));
		}
		
		public string Create(
			TemplateDeliveryScheduleBaseDto deliverySchedule,
			TimeSpan fromInterval,
			TimeSpan toInterval,
			bool withStartedAt = true
			)
		{
			var from = $"{fromInterval:hh\\:mm}";
			var to = $"{toInterval:hh\\:mm}";
			
			switch(deliverySchedule.Type)
			{
				case CustomerTemplateDeliveryScheduleType.Weekly:
					return GenerateWeeklyDescription(deliverySchedule, from, to, withStartedAt);
				case CustomerTemplateDeliveryScheduleType.Monthly:
					return GenerateMonthlyDescription(deliverySchedule, from, to);
				default:
					throw new InvalidOperationException($"Неизвестный тип доставки автозаказа {deliverySchedule.Type}");
			}
		}
		
		public string Create(
			TemplateDeliveryScheduleData deliveryScheduleData,
			TimeSpan fromInterval,
			TimeSpan toInterval)
		{
			var deliverySchedule = deliveryScheduleData.ToTemplateDeliveryScheduleDto();
			return Create(deliverySchedule, fromInterval, toInterval, false);
		}

		private string GenerateWeeklyDescription(
			TemplateDeliveryScheduleBaseDto deliverySchedule,
			string fromInterval,
			string toInterval,
			bool withStartedAt = true
			)
		{
			if(!deliverySchedule.WeekDay.HasValue)
			{
				throw new InvalidOperationException("Не заполнен день доставки для недельного типа!");
			}

			var weekDay = deliverySchedule.WeekDay.ToWeekDayName();

			if(!deliverySchedule.RepeatEveryWeeks.HasValue)
			{
				throw new InvalidOperationException("Не заполнены повторы через определенное количество недель для недельного типа!");
			}

			if(!deliverySchedule.FirstDeliveryDate.HasValue)
			{
				throw new InvalidOperationException("Не заполнена дата первой доставки для недельного типа!");
			}

			var repeat = _repeatEveryWeeksConverter.Convert(deliverySchedule.RepeatEveryWeeks.Value);
			var startedAt = withStartedAt
				? $", начиная с {deliverySchedule.FirstDeliveryDate:dd.MM.yyyy}"
				: string.Empty;
			
			return $"По {weekDay.GetAttribute<AppellativeAttribute>()?.DativePlural.ToLower()} с {fromInterval} до {toInterval}," +
				$" {repeat}{startedAt}";
		}

		private string GenerateMonthlyDescription(
			TemplateDeliveryScheduleBaseDto deliverySchedule,
			string fromInterval,
			string toInterval
			)
		{
			if(!deliverySchedule.DayOfMonth.HasValue)
			{
				throw new InvalidOperationException("Не заполнено число для месячного типа!");
			}
			
			return $"{deliverySchedule.DayOfMonth} числа каждого месяца с {fromInterval} до {toInterval}";
		}
	}
}
