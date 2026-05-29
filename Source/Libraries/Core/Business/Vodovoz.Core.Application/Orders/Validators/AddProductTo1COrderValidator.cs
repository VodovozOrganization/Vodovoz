using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Validators
{
	public class AddProductTo1COrderValidator :
	{
		private IValidate _validate;
		
		public void SetNextHandler(AddProductTo1COrderValidator addProductTo1COrderValidator)
		{
			_nextHandler = addProductTo1COrderValidator;
		}
		
		public Result Validate(
			Nomenclature addingNomenclature,
			IAddProductSource source)
		{
			if(source.IsLoadedFrom1C)
			{
				return Result.Failure(new Error("CantAddProductTo1COrder", "Нельзя добавлять товары в заказ с 1С"));
			}

			if(_nextHandler != null)
			{
				return _nextHandler.Validate(addingNomenclature, source);
			}

			return Result.Success();
		}
	}
}
