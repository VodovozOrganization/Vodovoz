using System;
using System.Collections.Generic;
using System.Text;
using Core.Infrastructure;
using QS.Project.Journal;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class OnlineOrderTemplatesJournalNode : JournalEntityNodeBase
	{
		private string _weekdays;
		private string _weekdaysFromDb;

		/// <summary>
		/// Заголовок(для EntityEntry, EntityViewModelEntry)
		/// </summary>
		public override string Title => $"{OnlineOrderTemplate.TemplateTitle} №{Id}";
		/// <summary>
		/// Клиент
		/// </summary>
		public string CounterpartyName { get; set; }
		/// <summary>
		/// Адрес доставки
		/// </summary>
		public string CompiledAddress { get; set; }
		/// <summary>
		/// Телефон для связи
		/// </summary>
		public string ContactPhone { get; set; }
		/// <summary>
		/// Самовывоз
		/// </summary>
		public bool IsSelfDelivery { get; set; }
		/// <summary>
		/// Активен
		/// </summary>
		public bool IsActive { get; set; }
		/// <summary>
		/// В архиве
		/// </summary>
		public bool IsArchive { get; set; }
		/// <summary>
		/// Форма оплаты
		/// </summary>
		public OnlineOrderPaymentType PaymentType { get; set; }
		/// <summary>
		/// Периодичность доставки
		/// </summary>
		public OnlineOrderDeliveryFrequency DeliveryFrequency { get; set; }
		/// <summary>
		/// Время доставки
		/// </summary>
		public string DeliveryTime { get; set; }

		/// <summary>
		/// Дни недели из базы данных
		/// </summary>
		public string WeekdaysFromDB
		{
			get => _weekdaysFromDb;
			set
			{
				_weekdaysFromDb = value;
				UpdateWeekdays(_weekdaysFromDb);
			}
		}

		/// <summary>
		/// Номер последнего заказа из шаблона
		/// </summary>
		public int? LastOnlineOrderIdFromTemplate { get; set; }
		/// <summary>
		/// Дни недели по-русски
		/// </summary>
		public string Weekdays { get; private set; }
		
		private void UpdateWeekdays(string weekdays)
		{
			if(string.IsNullOrWhiteSpace(weekdays))
			{
				Weekdays = "Незаполненные дни недели";
				return;
			}
			
			var sb = new StringBuilder();
			var parsedWeekdays = WeekdaysFromDB.Split('\n');

			for(var i = 0; i < parsedWeekdays.Length; i++)
			{
				var weekday = parsedWeekdays[i];
				var weekdayEnum = weekday.TryParseAsEnum<WeekDayName>();

				if(i == parsedWeekdays.Length - 1)
				{
					sb.Append(weekdayEnum is null ? weekday : weekdayEnum.Value.GetEnumDisplayName());
					continue;
				}
					
				sb.AppendLine(weekdayEnum is null ? weekday : weekdayEnum.Value.GetEnumDisplayName());
			}
				
			Weekdays = sb.ToString();
		}
	}
}
