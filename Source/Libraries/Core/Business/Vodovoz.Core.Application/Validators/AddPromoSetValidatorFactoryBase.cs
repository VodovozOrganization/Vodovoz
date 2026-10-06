using System;
using System.Collections.Generic;
using Vodovoz.Core.Application.Validators.Rules;
using Vodovoz.Core.Application.Validators.Rules.PromoSets;
using Vodovoz.EntityRepositories.Orders;
using VodovozBusiness.Factories;
using VodovozBusiness.Rules;
using VodovozBusiness.Services.Orders;
using VodovozBusiness.Validation;
using VodovozBusiness.Validation.Rules;

namespace Vodovoz.Core.Application.Validators
{
	public abstract class AddPromoSetValidatorFactoryBase : IAddPromoSetValidatorFactory
	{
		protected AddPromoSetValidatorFactoryBase(
			IPromotionalSetRepository  promotionalSetRepository,
			IFreeLoaderChecker freeLoaderChecker
			)
		{
			PromotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			FreeLoaderChecker = freeLoaderChecker ?? throw new ArgumentNullException(nameof(freeLoaderChecker));
		}
		
		protected IPromotionalSetRepository PromotionalSetRepository { get; }
		protected IFreeLoaderChecker FreeLoaderChecker { get; }

		public abstract IAddPromoSetValidator Create();

		protected virtual IEnumerable<IAddPromoSetRule> AddCommonPromoSetRules(List<IAddPromoSetRule> addedPromoSetRules)
		{
			addedPromoSetRules.AddRange(new IAddPromoSetRule[]
			{
				new AddPromoSetToSelfDeliveryRule(),
				new AddPromoSetForNewClientsWithPreviousShipmentRule(FreeLoaderChecker),
				new AddPromoSetForNotNewClientsOrWithoutPreviousShipmentsRule(PromotionalSetRepository)
			});
			
			return addedPromoSetRules;
		}

		protected virtual IEnumerable<IAddNomenclatureToSaleRule> AddCommonPromoSetItemsRules()
		{
			return new IAddNomenclatureToSaleRule[]
			{
				new AddNonServiceItemToServiceSourceRule(),
				new AddServiceItemToNonServiceSourceRule()
			};
		}
	}
}
