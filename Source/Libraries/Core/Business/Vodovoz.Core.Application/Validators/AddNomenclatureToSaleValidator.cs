using System.Collections.Generic;
using Vodovoz.Core.Application.Validators.Rules;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Validation;
using VodovozBusiness.Validation.Rules;

namespace Vodovoz.Core.Application.Validators
{
	public class AddNomenclatureToSaleValidator : IAddNomenclatureToSaleValidator
	{
		public AddNomenclatureToSaleValidator(
			AddDepositSaleItemRule addDepositSaleItemRule,
			AddNonServiceItemToServiceSourceRule addNonServiceItemToServiceSourceRule,
			AddServiceItemToNonServiceSourceRule addServiceItemToNonServiceSourceRule,
			AddOnlineStoreSaleItemRule addOnlineStoreSaleItemRule,
			AddNomenclatureToSaleUnknownCounterpartyRule addNomenclatureToSaleUnknownCounterpartyRule,
			AddNomenclatureToDeliverySaleWithoutDeliveryPointRule addNomenclatureToDeliverySaleWithoutDeliveryPointRule
			)
		{
			SetRules(
				addDepositSaleItemRule,
				addNonServiceItemToServiceSourceRule,
				addServiceItemToNonServiceSourceRule,
				addOnlineStoreSaleItemRule,
				addNomenclatureToSaleUnknownCounterpartyRule,
				addNomenclatureToDeliverySaleWithoutDeliveryPointRule);
		}

		protected IList<IAddSaleItemRule> AddSaleItemRules { get; private set; }
		protected IList<IAddNomenclatureToSaleRule> AddNomenclatureToSaleRules { get; private set; }

		public Result CanAddNomenclature(
			Nomenclature nomenclature,
			ISaleSource source
			
			)
		{
			foreach(var rule in AddNomenclatureToSaleRules)
			{
				var result = rule.Apply(nomenclature, source);
				
				if(result.IsFailure)
				{
					return result;
				}
			}

			return Result.Success();
		}
		
		public Result CanAddNomenclature(ISaleSource source)
		{
			foreach(var rule in AddSaleItemRules)
			{
				var result = rule.Apply(source);
				
				if(result.IsFailure)
				{
					return result;
				}
			}

			return Result.Success();
		}
		
		private void SetRules(
			AddDepositSaleItemRule addDepositSaleItemRule,
			AddNonServiceItemToServiceSourceRule addNonServiceItemToServiceSourceRule,
			AddServiceItemToNonServiceSourceRule addServiceItemToNonServiceSourceRule,
			AddOnlineStoreSaleItemRule addOnlineStoreSaleItemRule,
			AddNomenclatureToSaleUnknownCounterpartyRule addNomenclatureToSaleUnknownCounterpartyRule,
			AddNomenclatureToDeliverySaleWithoutDeliveryPointRule addNomenclatureToDeliverySaleWithoutDeliveryPointRule)
		{
			SetAddNomenclatureToSaleRules(
				addDepositSaleItemRule,
				addNonServiceItemToServiceSourceRule,
				addServiceItemToNonServiceSourceRule,
				addOnlineStoreSaleItemRule);

			SetSaleItemRules(addNomenclatureToSaleUnknownCounterpartyRule, addNomenclatureToDeliverySaleWithoutDeliveryPointRule);
		}

		private void SetSaleItemRules(
			AddNomenclatureToSaleUnknownCounterpartyRule addNomenclatureToSaleUnknownCounterpartyRule,
			AddNomenclatureToDeliverySaleWithoutDeliveryPointRule addNomenclatureToDeliverySaleWithoutDeliveryPointRule)
		{
			AddSaleItemRules = new List<IAddSaleItemRule>();

			AddSaleItemRules.Add(addNomenclatureToSaleUnknownCounterpartyRule);
			AddSaleItemRules.Add(addNomenclatureToDeliverySaleWithoutDeliveryPointRule);
		}

		private void SetAddNomenclatureToSaleRules(AddDepositSaleItemRule addDepositSaleItemRule,
			AddNonServiceItemToServiceSourceRule addNonServiceItemToServiceSourceRule,
			AddServiceItemToNonServiceSourceRule addServiceItemToNonServiceSourceRule, AddOnlineStoreSaleItemRule addOnlineStoreSaleItemRule)
		{
			AddNomenclatureToSaleRules = new List<IAddNomenclatureToSaleRule>();

			AddNomenclatureToSaleRules.Add(addDepositSaleItemRule);
			AddNomenclatureToSaleRules.Add(addNonServiceItemToServiceSourceRule);
			AddNomenclatureToSaleRules.Add(addServiceItemToNonServiceSourceRule);
			AddNomenclatureToSaleRules.Add(addOnlineStoreSaleItemRule);
		}
	}
}
