using System;
using System.Globalization;
using Vodovoz.Services;

namespace Vodovoz.Settings.Database.Wage
{
	public class WageSettings : IWageSettings
	{
		private readonly ISettingsController _settingsController;

		public WageSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int DaysWorkedForMinRatesLevel => _settingsController.GetIntValue("days_worked_for_min_rates_level");

		public DateTime DontRecalculateWagesForRouteListsBefore
		{
			get
			{
				var dateString = _settingsController.GetStringValue("dont_recalculate_wages_for_route_lists_before");
				if(!DateTime.TryParseExact(dateString, "dd.MM.yyyy", null, DateTimeStyles.None, out DateTime date))
				{
					throw new InvalidProgramException("В параметрах базы неверно заполнена дата до которой будет действовать запрет расчета зарплаты в МЛ (dont_recalculate_wages_for_route_lists_before)");
				}
				return date;
			}
		}

		public int SuburbWageDistrictId => _settingsController.GetIntValue("suburb_wage_district_id");

		public int CityWageDistrictId => _settingsController.GetIntValue("city_wage_district_id");
	}
}
