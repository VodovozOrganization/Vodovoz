using System;
using CustomerOrders.Contracts.V8.Sale.Templates;
using CustomerOrdersApi.Library.V8.Services.Converters;
using Gamma.Utilities;
using QS.DomainModel.Entity;

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
			TimeSpan toInterval)
		{
			var from = $"{fromInterval:hh\\:mm}";
			var to = $"{toInterval:hh\\:mm}";
			
			switch(deliverySchedule.Type)
			{
				case TemplateDeliveryScheduleType.Weekly:
					return GenerateWeeklyDescription(deliverySchedule, from, to);
				case TemplateDeliveryScheduleType.Monthly:
					return GenerateMonthlyDescription(deliverySchedule, from, to);
				default:
					throw new InvalidOperationException($"Неизвестный тип доставки автозаказа {deliverySchedule.Type}");
			}
		}

		private string GenerateWeeklyDescription(
			TemplateDeliveryScheduleBaseDto deliverySchedule,
			string fromInterval,
			string toInterval
			)
		{
			if(!deliverySchedule.WeekDay.HasValue)
			{
				throw new InvalidOperationException("Не заполнен день доставки для недельного типа!");
			}

			if(!deliverySchedule.RepeatEveryWeeks.HasValue)
			{
				throw new InvalidOperationException("Не заполнены повторы через определенное количество недель для недельного типа!");
			}

			if(!deliverySchedule.FirstDeliveryDate.HasValue)
			{
				throw new InvalidOperationException("Не заполнена дата первой доставки для недельного типа!");
			}

			var repeat = _repeatEveryWeeksConverter.Convert(deliverySchedule.RepeatEveryWeeks.Value);
			
			return $"По {deliverySchedule.WeekDay.GetAttribute<AppellativeAttribute>()?.DativePlural} с {fromInterval} до {toInterval}," +
				$" {repeat}, начиная с {deliverySchedule.FirstDeliveryDate:dd.MM.yyyy}";
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
