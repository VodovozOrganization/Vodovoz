using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Rules;
using VodovozBusiness.Validation;
using VodovozBusiness.Validation.Rules;

namespace Vodovoz.Core.Application.Validators
{
	public class AddPromoSetValidator : IAddPromoSetValidator
	{
		private readonly IEnumerable<IAddPromoSetRule> _addPromoSetRules;
		private readonly IEnumerable<IAddNomenclatureToSaleRule> _addPromoSetItemsRules;
		private readonly IEnumerable<IAddSaleItemRule> _addSaleItemRules;

		public AddPromoSetValidator(
			IEnumerable<IAddPromoSetRule> addPromoSetRules,
			IEnumerable<IAddNomenclatureToSaleRule> addPromoSetItemsRules,
			IEnumerable<IAddSaleItemRule> addSaleItemRules = null)
		{
			_addPromoSetRules = addPromoSetRules ?? throw new ArgumentNullException(nameof(addPromoSetRules));
			_addPromoSetItemsRules = addPromoSetItemsRules ?? throw new ArgumentNullException(nameof(addPromoSetItemsRules));
			_addSaleItemRules = addSaleItemRules ?? new List<IAddSaleItemRule>();
		}
		
		/// <summary>
		/// Проверка на возможность добавления промонабора
		/// </summary>
		/// <returns><c>Result.Success</c>, если можно добавить промонабор,
		/// <c>Result.Failure</c> если нельзя.</returns>
		/// <param name="uow">unit of work</param>
		/// <param name="source">Источник, куда добавляется промонабор</param>
		/// <param name="proSet">Промонабор</param>
		public virtual Result<string> CanAddPromotionalSet(
			IUnitOfWork uow,
			ISaleSource source,
			PromotionalSet proSet
			)
		{
			foreach(var addSaleItemRule in _addSaleItemRules)
			{
				var result = addSaleItemRule.Apply(source);

				if(result.IsFailure)
				{
					return Result.Failure<string>(result.Errors);
				}
			}

			foreach(var proSetItem in proSet.PromotionalSetItems)
			{
				foreach(var addPromoSetItemsRule in _addPromoSetItemsRules)
				{
					var result = addPromoSetItemsRule.Apply(proSetItem.Nomenclature, source);

					if(result.IsFailure)
					{
						return Result.Failure<string>(result.Errors);
					}
				}
			}
			
			foreach(var rule in _addPromoSetRules)
			{
				var result = rule.Apply(uow, source, proSet);

				if(result != null)
				{
					return result;
				}
			}
			
			return Result.Success(string.Empty);
		}
	}
}
