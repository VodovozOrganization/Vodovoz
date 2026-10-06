using System.Collections.Generic;
using Vodovoz.Core.Application.Validators.Rules;
using Vodovoz.Core.Application.Validators.Rules.PromoSets;
using Vodovoz.EntityRepositories.Orders;
using VodovozBusiness.Rules;
using VodovozBusiness.Services.Orders;
using VodovozBusiness.Validation;
using VodovozBusiness.Validation.Rules;

namespace Vodovoz.Core.Application.Validators
{
	public class OrderAddPromoSetValidatorFactory : AddPromoSetValidatorFactoryBase
	{
		public OrderAddPromoSetValidatorFactory(
			IPromotionalSetRepository promotionalSetRepository,
			IFreeLoaderChecker freeLoaderChecker
		) : base(promotionalSetRepository, freeLoaderChecker)
		{
		}
		
		public override IAddPromoSetValidator Create()
		{
			var addSaleItemsRules = new List<IAddSaleItemRule>
			{
				new AddSaleItemTo1COrderRule()
			};

			var addPromoSetItemsRules = AddCommonPromoSetItemsRules();
			
			var addPromoSetRules = AddCommonPromoSetRules(
				new List<IAddPromoSetRule>
				{
					new AddMoreOnePromoSetForNewClientsToOrderRule()
				});

			return new AddPromoSetValidator(addPromoSetRules, addPromoSetItemsRules, addSaleItemsRules);
		}
	}
}
