using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sale;

namespace Vodovoz.EntityRepositories.Sale
{
	public interface IDistrictRuleRepository
	{
		/// <summary>
		/// Проверка существования похожего правила доставки с теми же настройками
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="rule">Создаваемое правило</param>
		/// <returns>true - если уже есть такие правила, false - если нет</returns>
		bool SameDeliveryPriceRuleExists(IUnitOfWork uow, DeliveryPriceRule rule);
		IList<CommonDistrictRuleItem> GetCommonDistrictRuleItemsForDistrict(IUnitOfWork uow, District district);
		List<DistrictAndDistrictSet> GetDistrictNameDistrictSetNameAndCreationDateByDeliveryPriceRule(IUnitOfWork uow, DeliveryPriceRule rule);
		IList<District> GetDistrictsHavingRule(IUnitOfWork uow, DeliveryPriceRule rule);
	}
}
