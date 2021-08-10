using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Sectors
{
	[HistoryTrace]
	public class SectorWeekDayDeliveryRule: PropertyChangedBase, IDomainObject, ICloneable
	{
		public virtual string Title => $"{DeliveryPriceRule}, то цена {Price.ToString("C0", CultureInfo.CreateSpecificCulture("ru-RU"))}";
		
		public int Id { get; set; }

		private DateTime _startDate;
		
		[Display(Name = "Время создания")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		private DateTime _endDate;

		[Display(Name = "Время закрытия")]
		public virtual DateTime EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		private SectorWeekDayDeliveryRuleVersion _sectorWeekDayDeliveryRuleVersion;

		public SectorWeekDayDeliveryRuleVersion SectorWeekDayDeliveryRuleVersion
		{
			get => _sectorWeekDayDeliveryRuleVersion;
			set => SetField(ref _sectorWeekDayDeliveryRuleVersion, value);
		}

		private WeekDayName _deliveryWeekDay;

		public WeekDayName DeliveryWeekDay
		{
			get => _deliveryWeekDay;
			set => SetField(ref _deliveryWeekDay, value);
		}
		
		private DeliveryPriceRule _deliveryPriceRule;
		[Display(Name = "Правило цены доставки")]
		public virtual DeliveryPriceRule DeliveryPriceRule {
			get => _deliveryPriceRule;
			set => SetField(ref _deliveryPriceRule, value);
		}

		private decimal _price;
		[Display(Name = "Цена доставки")]
		public virtual decimal Price {
			get => _price;
			set => SetField(ref _price, value);
		}

		public object Clone()
		{
			var sectorWeekDayDeliveryRuleVersionClone = SectorWeekDayDeliveryRuleVersion.Clone() as SectorWeekDayDeliveryRuleVersion;
			var deliveryPriceRuleClone = DeliveryPriceRule.Clone() as DeliveryPriceRule;
			
			return new SectorWeekDayDeliveryRule
			{
				StartDate = StartDate,
				EndDate = EndDate,
				SectorWeekDayDeliveryRuleVersion = sectorWeekDayDeliveryRuleVersionClone,
				DeliveryPriceRule = deliveryPriceRuleClone,
				Price = Price,
				DeliveryWeekDay = DeliveryWeekDay
			};
		}
	}
}