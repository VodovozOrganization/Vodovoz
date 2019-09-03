using System;
using QS.DomainModel.UoW;
using QSSupportLib;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.StoredResources;
using Vodovoz.Services;

namespace Vodovoz.Core.DataService
{
	public class BaseParametersProvider : IStandartNomenclatures , IImageProvider, IStandartDiscountsService , IPersonProvider , ISubdivisionService
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

		public Employee GetDefaultEmployeeForCallTask(IUnitOfWork uow)
		{
			if(!MainSupport.BaseParameters.All.ContainsKey("сотрудник_по_умолчанию_для_crm")) {
				throw new InvalidProgramException("В параметрах базы не настроен параметр сотрудник по умолчанию для crm (сотрудник_по_умолчанию_для_crm).");
			}
			int employeeId = int.Parse(MainSupport.BaseParameters.All["сотрудник_по_умолчанию_для_crm"]);

			return uow.GetById<Employee>(employeeId);
		}

		public Employee GetDefaultEmployeeForDepositReturnTask(IUnitOfWork uow)
		{
			if(!MainSupport.BaseParameters.All.ContainsKey("сотрудник_по_умолчанию_для_задач_по_залогам")) {
				throw new InvalidProgramException("В параметрах базы не настроен параметр сотрудник по умолчанию для crm (сотрудник_по_умолчанию_для_задач_по_залогам).");
			}
			int employeeId = int.Parse(MainSupport.BaseParameters.All["сотрудник_по_умолчанию_для_задач_по_залогам"]);

			return uow.GetById<Employee>(employeeId);
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
	}
}
