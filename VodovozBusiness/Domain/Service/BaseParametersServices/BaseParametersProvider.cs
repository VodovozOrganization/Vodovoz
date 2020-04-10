using System;
using Vodovoz.Services;
using System.Globalization;
using Vodovoz.Parameters;
using QSSupportLib;
using System.Collections.Generic;

namespace Vodovoz.Core.DataService
{
	public class BaseParametersProvider : 
		IStandartNomenclatures, 
		IImageProvider, 
		IStandartDiscountsService, 
		IPersonProvider,
		ICommonParametersProvider, 
		ISmsNotifierParametersProvider,
		IWageParametersProvider,
		IDefaultDeliveryDaySchedule,
		ISmsNotificationServiceSettings,
		ISalesReceiptsServiceSettings,
		IEmailServiceSettings,
		IContactsParameters,
		IDriverServiceParametersProvider,
		IErrorSendParameterProvider,
		IPotentialFreePromosetsReportDefaultsProvider
	{
		public string GetDefaultBaseForErrorSend()
		{
			if(!MainSupport.BaseParameters.All.ContainsKey("base_for_error_send")) {
				throw new InvalidProgramException("В параметрах базы не настроена база для отправки сообщений об ошибку (base_for_error_send).");
			}
			return MainSupport.BaseParameters.All["base_for_error_send"];
		}

		public int GetRowCountForErrorLog()
		{
			if(!ParametersProvider.Instance.ContainsParameter("row_count_for_error_log")) {
				throw new InvalidProgramException("В параметрах базы не настроено кол-во строк для лога сообщения об ошибке(row_count_for_error_log).");
			}
			return int.Parse(ParametersProvider.Instance.GetParameterValue("row_count_for_error_log"));
		}

		public int GetForfeitId()
		{
			if(!ParametersProvider.Instance.ContainsParameter("forfeit_nomenclature_id")) {
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура бутыли по умолчанию (forfeit_nomenclature_id).");
			}
			return int.Parse(ParametersProvider.Instance.GetParameterValue("forfeit_nomenclature_id"));
		}

		public int GetDiscountForStockBottle()
		{
			if(!ParametersProvider.Instance.ContainsParameter("причина_скидки_для_акции_Бутыль")) {
				throw new InvalidProgramException("В параметрах базы не настроен параметр основания скидки для акции Бутыль (причина_скидки_для_акции_Бутыль).");
			}
			return int.Parse(ParametersProvider.Instance.GetParameterValue("причина_скидки_для_акции_Бутыль"));
		}

		public int GetDefaultEmployeeForCallTask()
		{
			if(!ParametersProvider.Instance.ContainsParameter("сотрудник_по_умолчанию_для_crm")) {
				throw new InvalidProgramException("В параметрах базы не настроен параметр сотрудник по умолчанию для crm (сотрудник_по_умолчанию_для_crm).");
			}
			return int.Parse(ParametersProvider.Instance.GetParameterValue("сотрудник_по_умолчанию_для_crm")); ;
		}

		public int GetDefaultEmployeeForDepositReturnTask()
		{
			if(!ParametersProvider.Instance.ContainsParameter("сотрудник_по_умолчанию_для_задач_по_залогам")) {
				throw new InvalidProgramException("В параметрах базы не настроен параметр сотрудник по умолчанию для crm (сотрудник_по_умолчанию_для_задач_по_залогам).");
			}
			return int.Parse(ParametersProvider.Instance.GetParameterValue("сотрудник_по_умолчанию_для_задач_по_залогам"));
		}

		public int GetCrmIndicatorId()
		{
			if(!ParametersProvider.Instance.ContainsParameter("crm_importance_indicator_id")) {
				throw new InvalidProgramException("В параметрах базы не настроен индикатор важности задачи для CRM (crm_importance_indicator_id).");
			}
			return int.Parse(ParametersProvider.Instance.GetParameterValue("crm_importance_indicator_id"));
		}

		public bool UseOldAutorouting()
		{
			if(!ParametersProvider.Instance.ContainsParameter("use_old_autorouting") || !bool.TryParse(ParametersProvider.Instance.GetParameterValue("use_old_autorouting"), out bool res))
				return false;
			return res;
		}

		#region ISmsNotifierParameters implementation

		public bool IsSmsNotificationsEnabled {
			get {
				if(!ParametersProvider.Instance.ContainsParameter("is_sms_notification_enabled")) {
					throw new InvalidProgramException("В параметрах базы не настроен параметр для включения смс уведомлений (is_sms_notification_enabled).");
				}
				string value = ParametersProvider.Instance.GetParameterValue("is_sms_notification_enabled");
				if(value == "true" || value == "1") {
					return true;
				}
				return false;
			}
		}

		public string GetNewClientSmsTextTemplate()
		{
			if(!ParametersProvider.Instance.ContainsParameter("new_client_sms_text_template")) {
				throw new InvalidProgramException("В параметрах базы не настроен шаблон для смс уведомлений новых клиентов (new_client_sms_text_template).");
			}
			return ParametersProvider.Instance.GetParameterValue("new_client_sms_text_template");
		}

		public decimal GetLowBalanceLevel()
		{
			if(!ParametersProvider.Instance.ContainsParameter("low_balance_level_for_sms_notifications")) {
				throw new InvalidProgramException("В параметрах базы не указан минимальный уровень средств на счете при котором будет отправляться уведомление о низком уровне средст на счете (low_balance_level_for_sms_notifications).");
			}
			string balanceString = ParametersProvider.Instance.GetParameterValue("low_balance_level_for_sms_notifications");

			if(!decimal.TryParse(balanceString, out decimal balance)) {
				throw new InvalidProgramException("В параметрах базы неверно заполнен (невозможно преобразовать в число) минимальный уровень средств на счете при котором будет отправляться уведомление о низком уровне средст на счете (low_balance_level_for_sms_notifications)");
			}
			return balance;
		}

		public string GetLowBalanceNotifiedPhone()
		{
			if(!ParametersProvider.Instance.ContainsParameter("low_balance_sms_notified_phone")) {
				throw new InvalidProgramException("В параметрах базы не настроен телефон на который будут отправляться сообщения о низком балансе денежных средств на счете для отпарвки смс уведомлений (low_balance_sms_notified_phone).");
			}
			return ParametersProvider.Instance.GetParameterValue("low_balance_sms_notified_phone");
		}

		public string GetLowBalanceNotifyText()
		{
			if(!ParametersProvider.Instance.ContainsParameter("low_balance_sms_notify_text")) {
				throw new InvalidProgramException("Текст сообщения о низком балансе средств на счете для отправки смс уведомлений (low_balance_sms_notify_text).");
			}
			return ParametersProvider.Instance.GetParameterValue("low_balance_sms_notify_text");
		}

		#endregion ISmsNotifierParameters implementation

		#region IWageParametersProvider implementation

		public int GetDaysWorkedForMinRatesLevel()
		{
			if(!ParametersProvider.Instance.ContainsParameter("days_worked_for_min_rates_level")) {
				throw new InvalidProgramException("В параметрах базы не указано количество дней которые новый водитель должен отработать для автоматической смены расчета зарплаты на уровневый расчет (days_worked_for_min_rates_level).");
			}
			string balanceString = ParametersProvider.Instance.GetParameterValue("days_worked_for_min_rates_level");

			if(!int.TryParse(balanceString, out int days)) {
				throw new InvalidProgramException("В параметрах базы неверно заполнено (невозможно преобразовать в число) количество дней которые новый водитель должен отработать для автоматической смены расчета зарплаты на уровневый расчет (fixed_wage_for_new_largus_drivers)");
			}
			return days;
		}

		public decimal GetFixedWageForNewLargusDrivers()
		{
			if(!ParametersProvider.Instance.ContainsParameter("fixed_wage_for_new_largus_drivers")) {
				throw new InvalidProgramException("В параметрах базы не указана фикса для новых водителей ларгусов (fixed_wage_for_new_largus_drivers).");
			}
			string balanceString = ParametersProvider.Instance.GetParameterValue("fixed_wage_for_new_largus_drivers");

			if(!decimal.TryParse(balanceString, NumberStyles.Number, CultureInfo.CreateSpecificCulture("ru-RU"), out decimal balance)) {
				throw new InvalidProgramException("В параметрах базы неверно заполнена (невозможно преобразовать в число) фикса для новых водителей ларгусов (fixed_wage_for_new_largus_drivers)");
			}
			return balance;
		}

		#endregion IWageParametersProvider implementation


		#region ISmsNotificationServiceSettings implementation

		public int MaxUnsendedSmsNotificationsForWorkingService {
			get {
				if(!ParametersProvider.Instance.ContainsParameter("MaxUnsendedSmsNotificationsForWorkingService")) {
					throw new InvalidProgramException("В параметрах базы не заполнено максимальное количество неотправленных смс уведомлений для рабочей службы (MaxUnsendedSmsNotificationsForWorkingService).");
				}
				string value = ParametersProvider.Instance.GetParameterValue("MaxUnsendedSmsNotificationsForWorkingService");

				if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result)) {
					throw new InvalidProgramException("В параметрах базы неверно заполнено максимальное количество неотправленных смс уведомлений для рабочей службы (MaxUnsendedSmsNotificationsForWorkingService)");
				}

				return result;
			}
		}

		#endregion ISmsNotificationServiceSettings implementation

		#region ISalesReceiptsServiceSettings implementation

		public int MaxUnsendedCashReceiptsForWorkingService {
			get {
				if(!ParametersProvider.Instance.ContainsParameter("MaxUnsendedCashReceiptsForWorkingService")) {
					throw new InvalidProgramException("В параметрах базы не заполнено максимальное количество неотправленных кассовых чеков для рабочей службы (MaxUnsendedCashReceiptsForWorkingService).");
				}
				string value = ParametersProvider.Instance.GetParameterValue("MaxUnsendedCashReceiptsForWorkingService");

				if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result)) {
					throw new InvalidProgramException("В параметрах базы неверно заполнено максимальное количество неотправленных кассовых чеков для рабочей службы (MaxUnsendedCashReceiptsForWorkingService)");
				}

				return result;
			}
		}

		#endregion ISalesReceiptsServiceSettings implementation

		#region IEmailServiceSettings implementation

		public int MaxEmailsInQueueForWorkingService {
			get {
				if(!ParametersProvider.Instance.ContainsParameter("MaxEmailsInQueueForWorkingService")) {
					throw new InvalidProgramException("В параметрах базы не заполнено максимальное количество писем в очереди на отправку для рабочей службы (MaxEmailsInQueueForWorkingService).");
				}
				string value = ParametersProvider.Instance.GetParameterValue("MaxEmailsInQueueForWorkingService");

				if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result)) {
					throw new InvalidProgramException("В параметрах базы неверно заполнено максимальное количество писем в очереди на отправку для рабочей службы (MaxEmailsInQueueForWorkingService)");
				}

				return result;
			}
		}

		#endregion IEmailServiceSettings implementation

		#region IContactsParameters

		public int MinSavePhoneLength {
			get {
				if(!ParametersProvider.Instance.ContainsParameter("MinSavePhoneLength")) {
					throw new InvalidProgramException("В параметрах базы не заполнено значение минимальной длины телефонного номера (MinSavePhoneLength)");
				}
				string value = ParametersProvider.Instance.GetParameterValue("MinSavePhoneLength");

				if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result)) {
					throw new InvalidProgramException("В параметрах базы неверно заполнено значение минимальной длины телефонного номера (MinSavePhoneLength)");
				}

				return result;
			}
	 	}
		public string DefaultCityCode {
			get {
				if(!ParametersProvider.Instance.ContainsParameter("default_city_code")) {
					throw new InvalidProgramException("В параметрах базы не заполнено значение стандартного кода города (default_city_code)");
				}

				string value = ParametersProvider.Instance.GetParameterValue("default_city_code");

				if(string.IsNullOrWhiteSpace(value)) {
					throw new InvalidProgramException("В параметрах базы неверно заполнено значение стандартного кода города (default_city_code)");
				}

				return value;
			}
		}

		#endregion

		#region IDriverServiceParametersProvider

		public int MaxUoWAllowed {
			get {
				if(!ParametersProvider.Instance.ContainsParameter("max_uow_allowed")) {
					throw new InvalidProgramException("В параметрах базы не заполнено значение максимального количества UoW (max_uow_allowed)");
				}

				string value = ParametersProvider.Instance.GetParameterValue("max_uow_allowed");

				if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result)) {
					throw new InvalidProgramException("В параметрах базы неверно заполнено значение максимального количества UoW (max_uow_allowed)");
				}

				return result;
			}
		}

		#endregion

		#region IDefaultDeliveryDaySchedule

		public int GetDefaultDeliveryDayScheduleId()
		{
			if(!ParametersProvider.Instance.ContainsParameter("default_delivery_day_schedule_id")) {
				throw new InvalidProgramException("В параметрах базы не настроена смена работы по умолчанию (default_delivery_day_schedule_id).");
			}
			return int.Parse(ParametersProvider.Instance.GetParameterValue("default_delivery_day_schedule_id"));
		}

		#endregion

		#region IDefaultDeliveryDaySchedule

		public int[] GetDefaultActivePromosets()
		{
			if(!ParametersProvider.Instance.ContainsParameter("default_active_promosets_in_potential_free_promosets_report")) {
				return new int[] { };
			}

			string value = ParametersProvider.Instance.GetParameterValue("default_active_promosets_in_potential_free_promosets_report");
			if(string.IsNullOrWhiteSpace(value)) {
				return new int[] { };
			}
			var values = value.Split(',');

			List<int> result = new List<int>();
			foreach(var v in values) {
				if(!int.TryParse(v.Trim(), out int parseResult)) {
					continue;
				}
				result.Add(parseResult);
			}
			return result.ToArray();
		}

		#endregion IDefaultDeliveryDaySchedule
	}
}
