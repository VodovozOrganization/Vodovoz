using Vodovoz.Domain.Sale;
using VodovozBusiness.Domain.Service;

namespace VodovozBusiness.Domain.Orders
{
	public class WeekDayServiceDistrictRule : ServiceDistrictRule
	{
		private WeekDayName _week_day;

		public virtual WeekDayName WeekDay
		{
			get => _week_day;
			set => SetField(ref _week_day, value);
		}

		public override object Clone()
		{
			var newWeekDayDistrictRuleItem = new WeekDayServiceDistrictRule
			{
				WeekDay = WeekDay,
				Price = Price,
				ServiceDistrict = ServiceDistrict,
				ServiceType = ServiceType
			};

			return newWeekDayDistrictRuleItem;
		}
	}
}
