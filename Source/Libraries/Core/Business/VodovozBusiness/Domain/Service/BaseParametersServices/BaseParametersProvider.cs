using System;
using System.Collections.Generic;
using System.Globalization;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.Core.DataService
{
	public class BaseParametersProvider : 
		IStandartNomenclatures, 
		IImageProvider, 
		IStandartDiscountsService, 
		IPersonProvider, 
		ISmsNotifierParametersProvider,
		IWageParametersProvider,
		IDefaultDeliveryDayScheduleSettings,
		ISmsNotificationServiceSettings,
		ISalesReceiptsServiceSettings,
		IEmailServiceSettings,
		IDriverServiceParametersProvider,
		IErrorSendParameterProvider,
		IProfitCategoryProvider,
		IMailjetParametersProvider,
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

		#region IErrorSendParameterProvider

		public string GetDefaultBaseForErrorSend()
		{
			if(!_parametersProvider.ContainsParameter("base_for_error_send"))
			{
				throw new InvalidProgramException("В параметрах базы не настроена база для отправки сообщений об ошибку (base_for_error_send).");
			}
			return _parametersProvider.GetParameterValue("base_for_error_send");
		}

		public int GetRowCountForErrorLog()
		{
			if(!_parametersProvider.ContainsParameter("row_count_for_error_log"))
			{
				throw new InvalidProgramException("В параметрах базы не настроено кол-во строк для лога сообщения об ошибке(row_count_for_error_log).");
			}
			return int.Parse(_parametersProvider.GetParameterValue("row_count_for_error_log"));
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

		#region IPersonProvider

		public int GetDefaultEmployeeForCallTask()
		{
			if(!_parametersProvider.ContainsParameter("сотрудник_по_умолчанию_для_crm"))
			{
				throw new InvalidProgramException("В параметрах базы не настроен параметр сотрудник по умолчанию для crm (сотрудник_по_умолчанию_для_crm).");
			}
			return int.Parse(_parametersProvider.GetParameterValue("сотрудник_по_умолчанию_для_crm")); ;
		}

		public int GetDefaultEmployeeForDepositReturnTask()
		{
			if(!_parametersProvider.ContainsParameter("сотрудник_по_умолчанию_для_задач_по_залогам"))
			{
				throw new InvalidProgramException("В параметрах базы не настроен параметр сотрудник по умолчанию для crm (сотрудник_по_умолчанию_для_задач_по_залогам).");
			}
			return int.Parse(_parametersProvider.GetParameterValue("сотрудник_по_умолчанию_для_задач_по_залогам"));
		}

		#endregion

		#region IImageProvider

		public int GetCrmIndicatorId()
		{
			if(!_parametersProvider.ContainsParameter("crm_importance_indicator_id"))
			{
				throw new InvalidProgramException("В параметрах базы не настроен индикатор важности задачи для CRM (crm_importance_indicator_id).");
			}
			return int.Parse(_parametersProvider.GetParameterValue("crm_importance_indicator_id"));
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

		#region ISmsNotificationServiceSettings implementation

		public int MaxUnsendedSmsNotificationsForWorkingService
		{
			get
			{
				if(!_parametersProvider.ContainsParameter("MaxUnsendedSmsNotificationsForWorkingService"))
				{
					throw new InvalidProgramException("В параметрах базы не заполнено максимальное количество неотправленных смс уведомлений для рабочей службы (MaxUnsendedSmsNotificationsForWorkingService).");
				}
				string value = _parametersProvider.GetParameterValue("MaxUnsendedSmsNotificationsForWorkingService");

				if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result))
				{
					throw new InvalidProgramException("В параметрах базы неверно заполнено максимальное количество неотправленных смс уведомлений для рабочей службы (MaxUnsendedSmsNotificationsForWorkingService)");
				}

				return result;
			}
		}

		#endregion ISmsNotificationServiceSettings implementation

		#region ISalesReceiptsServiceSettings implementation

		public int MaxUnsendedCashReceiptsForWorkingService
		{
			get
			{
				if(!_parametersProvider.ContainsParameter("MaxUnsendedCashReceiptsForWorkingService"))
				{
					throw new InvalidProgramException("В параметрах базы не заполнено максимальное количество неотправленных кассовых чеков для рабочей службы (MaxUnsendedCashReceiptsForWorkingService).");
				}
				string value = _parametersProvider.GetParameterValue("MaxUnsendedCashReceiptsForWorkingService");

				if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result))
				{
					throw new InvalidProgramException("В параметрах базы неверно заполнено максимальное количество неотправленных кассовых чеков для рабочей службы (MaxUnsendedCashReceiptsForWorkingService)");
				}

				return result;
			}
		}
		
		public int DefaultSalesReceiptCashierId
		{
			get
			{
				if(!_parametersProvider.ContainsParameter("default_sales_receipt_cashier_id"))
				{
					throw new InvalidProgramException("В параметрах базы не заполнено значение Id кассира по умолчанию для службы отправки чеков (default_sales_receipt_cashier_id).");
				}
				string value = _parametersProvider.GetParameterValue("default_sales_receipt_cashier_id");

				if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result))
				{
					throw new InvalidProgramException("В параметрах базы неверно заполнено значение Id кассира по умолчанию для службы отправки чеков (default_sales_receipt_cashier_id)");
				}

				return result;
			}
		}

		#endregion ISalesReceiptsServiceSettings implementation

		#region IEmailServiceSettings implementation

		public int MaxEmailsInQueueForWorkingService {
			get
			{
				if(!_parametersProvider.ContainsParameter("MaxEmailsInQueueForWorkingService"))
				{
					throw new InvalidProgramException("В параметрах базы не заполнено максимальное количество писем в очереди на отправку для рабочей службы (MaxEmailsInQueueForWorkingService).");
				}
				string value = _parametersProvider.GetParameterValue("MaxEmailsInQueueForWorkingService");

				if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result))
				{
					throw new InvalidProgramException("В параметрах базы неверно заполнено максимальное количество писем в очереди на отправку для рабочей службы (MaxEmailsInQueueForWorkingService)");
				}

				return result;
			}
		}

		#endregion IEmailServiceSettings implementation

		#region IDriverServiceParametersProvider

		public int MaxUoWAllowed
		{
			get
			{
				if(!_parametersProvider.ContainsParameter("max_uow_allowed"))
				{
					throw new InvalidProgramException("В параметрах базы не заполнено значение максимального количества UoW (max_uow_allowed)");
				}

				string value = _parametersProvider.GetParameterValue("max_uow_allowed");

				if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result))
				{
					throw new InvalidProgramException("В параметрах базы неверно заполнено значение максимального количества UoW (max_uow_allowed)");
				}

				return result;
			}
		}

		#endregion

		#region IDefaultDeliveryDaySchedule

		public int GetDefaultDeliveryDayScheduleId()
		{
			if(!_parametersProvider.ContainsParameter("default_delivery_day_schedule_id"))
			{
				throw new InvalidProgramException("В параметрах базы не настроена смена работы по умолчанию (default_delivery_day_schedule_id).");
			}
			return int.Parse(_parametersProvider.GetParameterValue("default_delivery_day_schedule_id"));
		}

		#endregion

		#region IProfitCategoryProvider

		public int GetDefaultProfitCategory()
		{
			if(!_parametersProvider.ContainsParameter("default_profit_category_id"))
			{
				throw new InvalidProgramException("В параметрах базы не настроена организация по умолчанию (default_profit_category_id).");
			}
			return int.Parse(_parametersProvider.GetParameterValue("default_profit_category_id"));
		}

		#endregion IProfitCategoryProvider

		#region IMailjetParametersProvider

		public string MailjetUserId
		{
			get
			{
				if(!_parametersProvider.ContainsParameter("MailjetUserId"))
				{
					throw new InvalidProgramException("В параметрах базы не указаны настройки подключения к серверу отправки почты Mailjet (MailjetUserId)");
				}
				return _parametersProvider.GetParameterValue("MailjetUserId");
			}
		}

		public string MailjetSecretKey {
			get {
				if(!_parametersProvider.ContainsParameter("MailjetSecretKey"))
				{
					throw new InvalidProgramException("В параметрах базы не указаны настройки подключения к серверу отправки почты Mailjet (MailjetSecretKey)");
				}
				return _parametersProvider.GetParameterValue("MailjetSecretKey");
			}
		}

		#endregion

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
