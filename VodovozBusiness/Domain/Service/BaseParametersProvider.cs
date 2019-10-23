using System;
using QSSupportLib;
using Vodovoz.Services;
using System.Globalization;

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
			if(!MainSupport.BaseParameters.All.ContainsKey("forfeit_nomenclature_id")) {
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура бутыли по умолчанию (forfeit_nomenclature_id).");
			}
			return int.Parse(MainSupport.BaseParameters.All["forfeit_nomenclature_id"]);
		}

		public int GetDiscountForStockBottle()
		{
			if(!MainSupport.BaseParameters.All.ContainsKey("причина_скидки_для_акции_Бутыль")) {
				throw new InvalidProgramException("В параметрах базы не настроен параметр основания скидки для акции Бутыль (причина_скидки_для_акции_Бутыль).");
			}
			return int.Parse(MainSupport.BaseParameters.All["причина_скидки_для_акции_Бутыль"]);
		}

		public int GetDefaultEmployeeForCallTask()
		{
			if(!MainSupport.BaseParameters.All.ContainsKey("сотрудник_по_умолчанию_для_crm")) {
				throw new InvalidProgramException("В параметрах базы не настроен параметр сотрудник по умолчанию для crm (сотрудник_по_умолчанию_для_crm).");
			}
			return int.Parse(MainSupport.BaseParameters.All["сотрудник_по_умолчанию_для_crm"]); ;
		}

		public int GetDefaultEmployeeForDepositReturnTask()
		{
			if(!MainSupport.BaseParameters.All.ContainsKey("сотрудник_по_умолчанию_для_задач_по_залогам")) {
				throw new InvalidProgramException("В параметрах базы не настроен параметр сотрудник по умолчанию для crm (сотрудник_по_умолчанию_для_задач_по_залогам).");
			}
			return int.Parse(MainSupport.BaseParameters.All["сотрудник_по_умолчанию_для_задач_по_залогам"]);
		}

		public int GetOkkId()
		{
			if(!MainSupport.BaseParameters.All.ContainsKey("номер_отдела_ОКК")) {
				throw new InvalidProgramException("В параметрах базы не настроен параметр : номер_отдела_ОКК");
			}
			return int.Parse(MainSupport.BaseParameters.All["номер_отдела_ОКК"]);
		}

		public int GetCrmIndicatorId()
		{
			if(!MainSupport.BaseParameters.All.ContainsKey("crm_importance_indicator_id")) {
				throw new InvalidProgramException("В параметрах базы не настроен индикатор важности задачи для CRM (crm_importance_indicator_id).");
			}
			return int.Parse(MainSupport.BaseParameters.All["crm_importance_indicator_id"]);
		}

		public bool UseOldAutorouting()
		{
			if(!MainSupport.BaseParameters.All.ContainsKey("use_old_autorouting") || !bool.TryParse(MainSupport.BaseParameters.All["use_old_autorouting"], out bool res))
				return false;
			return res;
		}

		#region ISmsNotifierParameters implementation

		public bool IsSmsNotificationsEnabled {
			get {
				MainSupport.LoadBaseParameters();
				if(!MainSupport.BaseParameters.All.ContainsKey("is_sms_notification_enabled")) {
					throw new InvalidProgramException("В параметрах базы не настроен параметр для включения смс уведомлений (is_sms_notification_enabled).");
				}
				string value = MainSupport.BaseParameters.All["is_sms_notification_enabled"];
				if(value == "true" || value == "1") {
					return true;
				}
				return false;
			}
		}

		public string GetNewClientSmsTextTemplate()
		{
			MainSupport.LoadBaseParameters();
			if(!MainSupport.BaseParameters.All.ContainsKey("new_client_sms_text_template")) {
				throw new InvalidProgramException("В параметрах базы не настроен шаблон для смс уведомлений новых клиентов (new_client_sms_text_template).");
			}
			return MainSupport.BaseParameters.All["new_client_sms_text_template"];
		}

		public decimal GetLowBalanceLevel()
		{
			MainSupport.LoadBaseParameters();
			if(!MainSupport.BaseParameters.All.ContainsKey("low_balance_level_for_sms_notifications")) {
				throw new InvalidProgramException("В параметрах базы не указан минимальный уровень средств на счете при котором будет отправляться уведомление о низком уровне средст на счете (low_balance_level_for_sms_notifications).");
			}
			string balanceString = MainSupport.BaseParameters.All["low_balance_level_for_sms_notifications"];

			if(!decimal.TryParse(balanceString, out decimal balance)) {
				throw new InvalidProgramException("В параметрах базы неверно заполнен (невозможно преобразовать в число) минимальный уровень средств на счете при котором будет отправляться уведомление о низком уровне средст на счете (low_balance_level_for_sms_notifications)");
			}
			return balance;
		}

		public string GetLowBalanceNotifiedPhone()
		{
			MainSupport.LoadBaseParameters();
			if(!MainSupport.BaseParameters.All.ContainsKey("low_balance_sms_notified_phone")) {
				throw new InvalidProgramException("В параметрах базы не настроен телефон на который будут отправляться сообщения о низком балансе денежных средств на счете для отпарвки смс уведомлений (low_balance_sms_notified_phone).");
			}
			return MainSupport.BaseParameters.All["low_balance_sms_notified_phone"];
		}

		public string GetLowBalanceNotifyText()
		{
			MainSupport.LoadBaseParameters();
			if(!MainSupport.BaseParameters.All.ContainsKey("low_balance_sms_notify_text")) {
				throw new InvalidProgramException("Текст сообщения о низком балансе средств на счете для отправки смс уведомлений (low_balance_sms_notify_text).");
			}
			return MainSupport.BaseParameters.All["low_balance_sms_notify_text"];
		}

		#endregion ISmsNotifierParameters implementation

		#region IWageParametersProvider implementation

		public decimal GetFixedWageForNewLargusDrivers()
		{
			MainSupport.LoadBaseParameters();
			if(!MainSupport.BaseParameters.All.ContainsKey("fixed_wage_for_new_largus_drivers")) {
				throw new InvalidProgramException("В параметрах базы не указана фикса для новых водителей ларгусов (fixed_wage_for_new_largus_drivers).");
			}
			string balanceString = MainSupport.BaseParameters.All["fixed_wage_for_new_largus_drivers"];

			if(!decimal.TryParse(balanceString, NumberStyles.Number, CultureInfo.CreateSpecificCulture("ru-RU"), out decimal balance)) {
				throw new InvalidProgramException("В параметрах базы неверно заполнена (невозможно преобразовать в число) фикса для новых водителей ларгусов (fixed_wage_for_new_largus_drivers)");
			}
			return balance;
		}

		#endregion IWageParametersProvider implementation
	}
}
