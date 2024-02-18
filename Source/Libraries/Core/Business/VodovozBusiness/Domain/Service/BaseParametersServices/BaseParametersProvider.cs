using System;
using System.Globalization;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.Core.DataService
{
	public class BaseParametersProvider : 
		IStandartDiscountsService, 
		IWageParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;
		
		public BaseParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		#region IStandartDiscountsService

		public int GetDiscountForStockBottle()
		{
			if(!_parametersProvider.ContainsParameter("причина_скидки_для_акции_Бутыль"))
			{
				throw new InvalidProgramException("В параметрах базы не настроен параметр основания скидки для акции Бутыль (причина_скидки_для_акции_Бутыль).");
			}
			return int.Parse(_parametersProvider.GetParameterValue("причина_скидки_для_акции_Бутыль"));
		}

		#endregion

		#region IWageParametersProvider implementation

		public int GetDaysWorkedForMinRatesLevel()
		{
			if(!_parametersProvider.ContainsParameter("days_worked_for_min_rates_level"))
			{
				throw new InvalidProgramException("В параметрах базы не указано количество дней которые новый водитель должен отработать для автоматической смены расчета зарплаты на уровневый расчет (days_worked_for_min_rates_level).");
			}
			string balanceString = _parametersProvider.GetParameterValue("days_worked_for_min_rates_level");

			if(!int.TryParse(balanceString, out int days))
			{
				throw new InvalidProgramException("В параметрах базы неверно заполнено (невозможно преобразовать в число) количество дней которые новый водитель должен отработать для автоматической смены расчета зарплаты на уровневый расчет (fixed_wage_for_new_largus_drivers)");
			}
			return days;
		}

		public decimal GetFixedWageForNewLargusDrivers()
		{
			if(!_parametersProvider.ContainsParameter("fixed_wage_for_new_largus_drivers"))
			{
				throw new InvalidProgramException("В параметрах базы не указана фикса для новых водителей ларгусов (fixed_wage_for_new_largus_drivers).");
			}
			string balanceString = _parametersProvider.GetParameterValue("fixed_wage_for_new_largus_drivers");

			if(!decimal.TryParse(balanceString, NumberStyles.Number, CultureInfo.CreateSpecificCulture("ru-RU"), out decimal balance))
			{
				throw new InvalidProgramException("В параметрах базы неверно заполнена (невозможно преобразовать в число) фикса для новых водителей ларгусов (fixed_wage_for_new_largus_drivers)");
			}
			return balance;
		}

		public DateTime DontRecalculateWagesForRouteListsBefore
		{
			get
			{
				if(!_parametersProvider.ContainsParameter("dont_recalculate_wages_for_route_lists_before"))
				{
					throw new InvalidProgramException("В параметрах базы не указана дата до которой будет действовать запрет расчета зарплаты в МЛ (dont_recalculate_wages_for_route_lists_before).");
				}
				string dateString = _parametersProvider.GetParameterValue("dont_recalculate_wages_for_route_lists_before");

				if(!DateTime.TryParseExact(dateString, "dd.MM.yyyy", null, DateTimeStyles.None, out DateTime date))
				{
					throw new InvalidProgramException("В параметрах базы неверно заполнена дата до которой будет действовать запрет расчета зарплаты в МЛ (dont_recalculate_wages_for_route_lists_before)");
				}
				return date;
			}
		}

		public int GetSuburbWageDistrictId
		{
			get
			{
				if(!_parametersProvider.ContainsParameter("suburb_wage_district_id"))
				{
					throw new InvalidProgramException(
						"В параметрах базы не указан код зарплатного района Пригород (suburb_wage_district_id).");
				}
				string idString = _parametersProvider.GetParameterValue("suburb_wage_district_id");

				if(!int.TryParse(idString, out int id))
				{
					throw new InvalidProgramException(
						"В параметрах базы неверно указан код зарплатного района Пригород (suburb_wage_district_id)");
				}
				return id;
			}
		}

		public int GetCityWageDistrictId
		{
			get
			{
				if(!_parametersProvider.ContainsParameter("city_wage_district_id"))
				{
					throw new InvalidProgramException(
						"В параметрах базы не указан код зарплатного района Город (city_wage_district_id).");
				}
				string idString = _parametersProvider.GetParameterValue("city_wage_district_id");

				if(!int.TryParse(idString, out int id))
				{
					throw new InvalidProgramException(
						"В параметрах базы неверно указан код зарплатного района Город (city_wage_district_id)");
				}
				return id;
			}
		}

		#endregion IWageParametersProvider implementation
	}
}
