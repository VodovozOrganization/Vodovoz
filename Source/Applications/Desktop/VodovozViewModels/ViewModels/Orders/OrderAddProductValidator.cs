using System;
using Vodovoz.Core.Application.Orders.Validators;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Orders;

public class OrderAddProductValidator : IAddProductValidator
{
	private readonly AddProductTo1COrderValidator _addProductTo1COrderValidator;
	private readonly AddDepositProductValidator _addDepositProductValidator;
	private readonly AddServiceProductToNonServiceSourceValidator _addServiceProductToNonServiceSourceValidator;
	private readonly AddNonServiceProductToServiceSourceValidator _addNonServiceProductToServiceSourceValidator;
	private readonly AddOnlineStoreProductValidator _addOnlineStoreProductValidator;

	public OrderAddProductValidator(
		AddProductTo1COrderValidator addProductTo1COrderValidator,
		AddDepositProductValidator addDepositProductValidator,
		AddServiceProductToNonServiceSourceValidator addServiceProductToNonServiceSourceValidator,
		AddNonServiceProductToServiceSourceValidator addNonServiceProductToServiceSourceValidator,
		AddOnlineStoreProductValidator addOnlineStoreProductValidator
	)
	{
		_addProductTo1COrderValidator = addProductTo1COrderValidator ?? throw new ArgumentNullException(nameof(addProductTo1COrderValidator));
		_addDepositProductValidator =
			addDepositProductValidator ?? throw new ArgumentNullException(nameof(addDepositProductValidator));
		_addServiceProductToNonServiceSourceValidator =
			addServiceProductToNonServiceSourceValidator ?? throw new ArgumentNullException(nameof(addServiceProductToNonServiceSourceValidator));
		_addNonServiceProductToServiceSourceValidator =
			addNonServiceProductToServiceSourceValidator ?? throw new ArgumentNullException(nameof(addNonServiceProductToServiceSourceValidator));
		_addOnlineStoreProductValidator =
			addOnlineStoreProductValidator ?? throw new ArgumentNullException(nameof(addOnlineStoreProductValidator));
	}

	public Result Validate(Nomenclature addingNomenclature, IAddProductSource source)
	{
		if(source.IsLoadedFrom1C)
		{
			return Result.Failure(new Error("CantAddProductTo1COrder", "Нельзя добавлять товары в заказ с 1С"));
		}

		var res = _addDepositProductValidator.Validate(addingNomenclature, source);
		if(res.IsFailure)
		{
			return res;
		}

		return Result.Success();
	}
}
