using System;
using QSSupportLib;
using Vodovoz.Services;

namespace Vodovoz.Core.DataService
{
	public class BaseParametersProvider : IStandartNomenclatures, IImageProvider, IStandartDiscountsService, IPersonProvider, ISubdivisionService, ICommonParametersProvider, ISmsNotifierParametersProvider
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

		public string GetNewClientSmsTextTemplate()
		{
			if(!MainSupport.BaseParameters.All.ContainsKey("new_client_sms_text_template")) {
				throw new InvalidProgramException("В параметрах базы не настроен шаблон для смс уведомлений новых клиентов (new_client_sms_text_template).");
			}
			return MainSupport.BaseParameters.All["new_client_sms_text_template"];
		}

		#endregion ISmsNotifierParameters implementation
	}
}
