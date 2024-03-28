using System.ComponentModel.DataAnnotations;
using System.Globalization;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "стоимости доставки дня недели района",
		Nominative = "стоимость доставки дня недели района")]
	public class WeekDayDistrictRuleItem : DistrictRuleItemBase
	{
		public virtual string Title => $"{DeliveryPriceRule}, то цена {Price.ToString("C0", CultureInfo.CreateSpecificCulture("ru-RU"))}";

		private WeekDayName _weekDay;

		[Display(Name = "День недели")]
		public virtual WeekDayName WeekDay
		{
			get => _weekDay;
			set => SetField(ref _weekDay, value);
		}

		public override object Clone()
		{
			var newWeekDayDistrictRuleItem = new WeekDayDistrictRuleItem
			{
				WeekDay = WeekDay,
				Price = Price,
				DeliveryPriceRule = DeliveryPriceRule
			};

			return newWeekDayDistrictRuleItem;
		}
	}
}
