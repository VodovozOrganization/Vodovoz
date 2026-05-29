using System;
using Vodovoz.Core.Application.Orders.Validators;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class AddProductFromPromoSetValidator : IAddProductValidator
	{
		private readonly AddServiceProductToNonServiceSourceValidator _addServiceProductToNonServiceSourceValidator;
		private readonly AddNonServiceProductToServiceSourceValidator _addNonServiceProductToServiceSourceValidator;
		
		public AddProductFromPromoSetValidator(
			AddServiceProductToNonServiceSourceValidator addServiceProductToNonServiceSourceValidator,
			AddNonServiceProductToServiceSourceValidator addNonServiceProductToServiceSourceValidator
			)
		{
			_addServiceProductToNonServiceSourceValidator =
				addServiceProductToNonServiceSourceValidator ?? throw new ArgumentNullException(nameof(addServiceProductToNonServiceSourceValidator));
			_addNonServiceProductToServiceSourceValidator =
				addNonServiceProductToServiceSourceValidator ?? throw new ArgumentNullException(nameof(addNonServiceProductToServiceSourceValidator));
		}
		
		public Result Validate(Nomenclature addingNomenclature, IAddProductSource source)
		{
			if(source.IsLoadedFrom1C)
			{
				return Result.Failure(new Error("CantAddProductTo1COrder", "Нельзя добавлять товары в заказ с 1С"));
			}

			var res = _addServiceProductToNonServiceSourceValidator.Validate(addingNomenclature, source);
			
			if(res.IsFailure)
			{
				return res;
			}

			return Result.Success();
		}
	}
}
