using System;
using QS.Services;
using Vodovoz.Core.Application.Orders.Validators;
using Vodovoz.Core.Application.Validators.Rules;
using VodovozBusiness.Factories;
using VodovozBusiness.Validation;

namespace Vodovoz.Core.Application.Validators
{
	public class AddNomenclatureToSaleValidatorFactory : IAddNomenclatureToSaleValidatorFactory
	{
		private readonly ICurrentPermissionService _currentPermissionService;

		public AddNomenclatureToSaleValidatorFactory(
			ICurrentPermissionService currentPermissionService)
		{
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
		}

		public IAddNomenclatureToSaleValidator Create()
		{
			return new AddNomenclatureToSaleValidator(
				new AddDepositSaleItemRule(),
				new AddNonServiceItemToServiceSourceRule(),
				new AddServiceItemToNonServiceSourceRule(),
				new AddOnlineStoreSaleItemRule(_currentPermissionService),
				new AddNomenclatureToSaleUnknownCounterpartyRule(),
				new AddNomenclatureToDeliverySaleWithoutDeliveryPointRule()
			);
		}
	}
}
