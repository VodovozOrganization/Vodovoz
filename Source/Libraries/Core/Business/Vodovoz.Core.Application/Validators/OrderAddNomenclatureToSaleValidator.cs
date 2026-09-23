using Vodovoz.Core.Application.Validators;
using Vodovoz.Core.Application.Validators.Rules;

namespace Vodovoz.Core.Application.Orders.Validators
{
	public class OrderAddNomenclatureToSaleValidator : AddNomenclatureToSaleValidator
	{
		public OrderAddNomenclatureToSaleValidator(
			AddSaleItemTo1COrderRule addSaleItemTo1COrderRule,
			AddDepositSaleItemRule addDepositSaleItemRule,
			AddNonServiceItemToServiceSourceRule addNonServiceItemToServiceSourceRule,
			AddServiceItemToNonServiceSourceRule addServiceItemToNonServiceSourceRule,
			AddOnlineStoreSaleItemRule addOnlineStoreSaleItemRule,
			AddNomenclatureToSaleUnknownCounterpartyRule addNomenclatureToSaleUnknownCounterpartyRule,
			AddNomenclatureToDeliverySaleWithoutDeliveryPointRule addNomenclatureToDeliverySaleWithoutDeliveryPointRule
		)
			: base(
				addDepositSaleItemRule,
				addNonServiceItemToServiceSourceRule,
				addServiceItemToNonServiceSourceRule,
				addOnlineStoreSaleItemRule,
				addNomenclatureToSaleUnknownCounterpartyRule,
				addNomenclatureToDeliverySaleWithoutDeliveryPointRule
			)
		{
			AddNomenclatureToSaleRules.Insert(0, addSaleItemTo1COrderRule);
		}
	}
}
