using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Results;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Validators
{
	public interface IAddNomenclatureToSaleValidator
	{
		Result CanAddNomenclatures(IAddSaleItemSource saleItemSource);
	}

	public class AddNomenclatureToSaleValidator : IAddNomenclatureToSaleValidator
	{
		private readonly IEnumerable<IAddNomenclatureToSaleRule> _rules;

		public AddNomenclatureToSaleValidator(IEnumerable<IAddNomenclatureToSaleRule> rules)
		{
			_rules = rules ?? throw new ArgumentNullException(nameof(rules));
		}

		public Result CanAddNomenclatures(IAddSaleItemSource saleItemSource)
		{
			foreach(var rule in _rules)
			{
				var result = rule.Validate(saleItemSource);
				
				if(result.IsFailure)
				{
					return result;
				}
			}

			return Result.Success();
		}
	}

	public interface IAddNomenclatureToSaleRule
	{
		Result Validate(IAddSaleItemSource saleItemSource);
	}

	public class AddNomenclatureToSaleUnknownCounterpartyRule : IAddNomenclatureToSaleRule
	{
		public Result Validate(IAddSaleItemSource saleItemSource)
		{
			if(saleItemSource.Counterparty is null)
			{
				Result.Failure();
			}

			return Result.Success();
		}
	}
	
	public class AddNomenclatureToDeliverySaleWithoutDeliveryPointRule : IAddNomenclatureToSaleRule
	{
		public Result Validate(IAddSaleItemSource saleItemSource)
		{
			if(saleItemSource.DeliveryPoint is null && !saleItemSource.IsSelfDelivery)
			{
				Result.Failure("Для добавления позиции на продажу должна быть выбрана точка доставки");
			}

			return Result.Success();
		}
	}
}
