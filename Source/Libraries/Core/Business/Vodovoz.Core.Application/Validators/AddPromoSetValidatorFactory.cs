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
	public class AddPromoSetValidatorFactory : IAddPromoSetValidatorFactory
	{
		private readonly IPromotionalSetRepository _promotionalSetRepository;
		private readonly IFreeLoaderChecker _freeLoaderChecker;

		public AddPromoSetValidatorFactory(
			IPromotionalSetRepository promotionalSetRepository,
			IFreeLoaderChecker freeLoaderChecker
		)
		{
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_freeLoaderChecker = freeLoaderChecker ?? throw new ArgumentNullException(nameof(freeLoaderChecker));
		}
		public IAddPromoSetValidator CreateForOrder()
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

		public IAddPromoSetValidator Create()
		{
			var addPromoSetRules = AddCommonPromoSetRules(
				new List<IAddPromoSetRule>
				{
					new AddMoreOnePromoSetForNewClientsRule(),
				});
			
			var addPromoSetItemsRules = AddCommonPromoSetItemsRules();

			return new AddPromoSetValidator(addPromoSetRules, addPromoSetItemsRules);
		}

		private IEnumerable<IAddPromoSetRule> AddCommonPromoSetRules(List<IAddPromoSetRule> addedPromoSetRules)
		{
			addedPromoSetRules.AddRange(new IAddPromoSetRule[]
			{
				new AddPromoSetToSelfDeliveryRule(),
				new AddPromoSetForNewClientsWithPreviousShipmentRule(_freeLoaderChecker),
				new AddPromoSetForNotNewClientsOrWithoutPreviousShipmentsRule(_promotionalSetRepository)
			});
			
			return addedPromoSetRules;
		}

		private IEnumerable<IAddNomenclatureToSaleRule> AddCommonPromoSetItemsRules()
		{
			return new IAddNomenclatureToSaleRule[]
			{
				new AddNonServiceItemToServiceSourceRule(),
				new AddServiceItemToNonServiceSourceRule()
			};
		}
	}
}
