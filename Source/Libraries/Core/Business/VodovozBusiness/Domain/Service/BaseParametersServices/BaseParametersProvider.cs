using System;
using System.Globalization;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.Core.DataService
{
	public class BaseParametersProvider : 
		IStandartNomenclatures, 
		IStandartDiscountsService, 
		ISmsNotifierParametersProvider,
		IWageParametersProvider,
		IVpbxSettings,
		ITerminalNomenclatureProvider
	{
		private readonly IParametersProvider _parametersProvider;
		
		public BaseParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}
		
		#region IStandartNomenclatures

		public int GetForfeitId()
		{
			if(!_parametersProvider.ContainsParameter("forfeit_nomenclature_id"))
			{
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура бутыли по умолчанию (forfeit_nomenclature_id).");
			}
			return int.Parse(_parametersProvider.GetParameterValue("forfeit_nomenclature_id"));
		}
		
		public int GetReturnedBottleNomenclatureId
		{
			get
			{
				if(!_parametersProvider.ContainsParameter("returned_bottle_nomenclature_id"))
				{
					throw new InvalidProgramException("В параметрах базы не заполнено значение id стандартной номенклатуры на возврат (returned_bottle_nomenclature_id)");
				}

				string value = _parametersProvider.GetParameterValue("returned_bottle_nomenclature_id");

				if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result))
				{
					throw new InvalidProgramException("В параметрах базы неверно заполнено значение id стандартной номенклатуры на возврат (returned_bottle_nomenclature_id)");
				}

				return result;
			}
		}

		#endregion

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

		#region ISmsNotifierParameters implementation

		public bool IsSmsNotificationsEnabled
		{
			get
			{
				if(!_parametersProvider.ContainsParameter("is_sms_notification_enabled"))
				{
					throw new InvalidProgramException("В параметрах базы не настроен параметр для включения смс уведомлений (is_sms_notification_enabled).");
				}
				string value = _parametersProvider.GetParameterValue("is_sms_notification_enabled");
				if(value == "true" || value == "1")
				{
					return true;
				}
				return false;
			}
		}

		public string GetNewClientSmsTextTemplate()
		{
			if(!_parametersProvider.ContainsParameter("new_client_sms_text_template"))
			{
				throw new InvalidProgramException("В параметрах базы не настроен шаблон для смс уведомлений новых клиентов (new_client_sms_text_template).");
			}
			return _parametersProvider.GetParameterValue("new_client_sms_text_template");
		}

		public decimal GetLowBalanceLevel()
		{
			if(!_parametersProvider.ContainsParameter("low_balance_level_for_sms_notifications"))
			{
				throw new InvalidProgramException("В параметрах базы не указан минимальный уровень средств на счете при котором будет отправляться уведомление о низком уровне средст на счете (low_balance_level_for_sms_notifications).");
			}
			string balanceString = _parametersProvider.GetParameterValue("low_balance_level_for_sms_notifications");

			if(!decimal.TryParse(balanceString, out decimal balance))
			{
				throw new InvalidProgramException("В параметрах базы неверно заполнен (невозможно преобразовать в число) минимальный уровень средств на счете при котором будет отправляться уведомление о низком уровне средст на счете (low_balance_level_for_sms_notifications)");
			}
			return balance;
		}

		public string GetLowBalanceNotifiedPhone()
		{
			if(!_parametersProvider.ContainsParameter("low_balance_sms_notified_phone"))
			{
				throw new InvalidProgramException("В параметрах базы не настроен телефон на который будут отправляться сообщения о низком балансе денежных средств на счете для отпарвки смс уведомлений (low_balance_sms_notified_phone).");
			}
			return _parametersProvider.GetParameterValue("low_balance_sms_notified_phone");
		}

		public string GetLowBalanceNotifyText()
		{
			if(!_parametersProvider.ContainsParameter("low_balance_sms_notify_text"))
			{
				throw new InvalidProgramException("Текст сообщения о низком балансе средств на счете для отправки смс уведомлений (low_balance_sms_notify_text).");
			}
			return _parametersProvider.GetParameterValue("low_balance_sms_notify_text");
		}
		
		public string GetUndeliveryAutoTransferNotApprovedTextTemplate()
		{
			if(!_parametersProvider.ContainsParameter("undelivery_autotransport_notapproved_sms_text_template")) 
			{
				throw new InvalidProgramException("В параметрах базы не настроен шаблон для смс уведомлений о переносе при недовозе  без согласования(undelivery_autotransport_notapproved_sms_text_template).");
			}
			return _parametersProvider.GetParameterValue("undelivery_autotransport_notapproved_sms_text_template");
		}

		#endregion ISmsNotifierParameters implementation

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

		#region ITerminalNomenclatureProvider

		public int GetNomenclatureIdForTerminal
		{
			get
			{
				if(!_parametersProvider.ContainsParameter("terminal_nomenclature_id")) {
					throw new InvalidProgramException("В параметрах базы не заполнено значение ключа номенклатуры терминал для оплаты (terminal_nomenclature_id)");
				}

				string value = _parametersProvider.GetParameterValue("terminal_nomenclature_id");

				if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result)) {
					throw new InvalidProgramException("В параметрах базы неверно заполнено значение ключа номенклатуры терминал для оплаты (terminal_nomenclature_id)");
				}

				return result;
			}
		}

		#endregion

		#region IVpbxSettings

		public string VpbxApiKey
		{ 
			get
			{
				if(!_parametersProvider.ContainsParameter("vpbx_api_key"))
				{
					throw new InvalidProgramException("В параметрах базы не настроены ключи доступа к Манго(vpbx_api_key).");
				}
				return _parametersProvider.GetParameterValue("vpbx_api_key");
			}
		}

		public string VpbxApiSalt
		{
			get
			{
				if(!_parametersProvider.ContainsParameter("vpbx_api_salt"))
				{
					throw new InvalidProgramException("В параметрах базы не настроены ключи доступа к Манго(vpbx_api_salt).");
				}
				return _parametersProvider.GetParameterValue("vpbx_api_salt");
			}
		}

		#endregion
	}
}
