using System;
using QS.Services;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Errors.Sale;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Validation.Rules;

namespace Vodovoz.Core.Application.Validators.Rules
{
	public class AddOnlineStoreSaleItemRule : IAddNomenclatureToSaleRule
	{
		private readonly bool _canAddOnlineStoreNomenclatures;
		
		public AddOnlineStoreSaleItemRule(ICurrentPermissionService currentPermissionService)
		{
			if(currentPermissionService is null)
			{
				throw new ArgumentNullException(nameof(currentPermissionService));
			}
			
			_canAddOnlineStoreNomenclatures = currentPermissionService
				.ValidatePresetPermission("can_add_online_store_nomenclatures_to_order");
		}
		
		public Result Apply(
			Nomenclature addingNomenclature,
			ISaleSource source
			)
		{
			if(addingNomenclature.OnlineStore != null && !_canAddOnlineStoreNomenclatures)
			{
				return Result.Failure(SaleErrors.DontHavePermissionsToAddOnlineStoreProductError());
			}

			return Result.Success();
		}
	}
}
