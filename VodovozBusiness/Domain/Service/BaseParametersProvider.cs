using System;
using QSSupportLib;
using Vodovoz.Services;
using System.Globalization;
using Vodovoz.Parameters;

namespace Vodovoz.Core.DataService
{
	public class BaseParametersProvider : 
		IStandartNomenclatures , 
		IImageProvider, 
		IStandartDiscountsService , 
		IPersonProvider , 
		ISubdivisionService,
		ICommonParametersProvider, 
		ISmsNotifierParametersProvider,
		IWageParametersProvider
	{
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

		public int GetOkkId()
		{
			if(!ParametersProvider.Instance.ContainsParameter("номер_отдела_ОКК")) {
				throw new InvalidProgramException("В параметрах базы не настроен параметр : номер_отдела_ОКК");
			}
			return int.Parse(ParametersProvider.Instance.GetParameterValue("номер_отдела_ОКК"));
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
	}
}
