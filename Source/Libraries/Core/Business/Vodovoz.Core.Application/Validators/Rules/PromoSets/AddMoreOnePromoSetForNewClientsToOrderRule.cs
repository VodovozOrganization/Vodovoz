using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Errors.Sale.PromoSets;
using VodovozBusiness.Rules;

namespace Vodovoz.Core.Application.Validators.Rules.PromoSets
{
	/// <summary>
	/// Правило добавления более одного промонабора для новых клиентов
	/// </summary>
	public class AddMoreOnePromoSetForNewClientsToOrderRule : IAddPromoSetRule
	{
		public Result<string> Apply(IUnitOfWork uow, ISaleSource source, PromotionalSet proSet)
		{
			var order = source as Order;
			
			if(order.PromotionalSets.Any(x => x.PromotionalSetForNewClients && proSet.PromotionalSetForNewClients))
			{
				return Result.Failure<string>(PromoSetErrors.CantAddTwoPromoSetsForNewClients);
			}
			
			return null;
		}
	}
}
