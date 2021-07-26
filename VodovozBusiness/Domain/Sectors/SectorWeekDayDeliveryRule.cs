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
		
		private Employee _author;
		[Display(Name = "Автор")]
		public virtual Employee Author {
			get => _author;
			set => SetField(ref _author, value);
		}

		private Employee _lastEditor;
		public virtual Employee LastEditor {
			get => _lastEditor;
			set => SetField(ref _lastEditor, value);
		}

		private SectorWeekDayRulesVersion _sectorWeekDayRulesVersion;

		public SectorWeekDayRulesVersion SectorWeekDayRulesVersion
		{
			get => _sectorWeekDayRulesVersion;
			set => SetField(ref _sectorWeekDayRulesVersion, value);
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
			var sectorWeekDayRulesVersionClone = SectorWeekDayRulesVersion.Clone() as SectorWeekDayRulesVersion;
			var deliveryPriceRuleClone = DeliveryPriceRule.Clone() as DeliveryPriceRule;
			
			return new SectorWeekDayDeliveryRule
			{
				SectorWeekDayRulesVersion = sectorWeekDayRulesVersionClone,
				DeliveryPriceRule = deliveryPriceRuleClone,
				Price = Price,
				DeliveryWeekDay = DeliveryWeekDay
			};
		}
	}
}