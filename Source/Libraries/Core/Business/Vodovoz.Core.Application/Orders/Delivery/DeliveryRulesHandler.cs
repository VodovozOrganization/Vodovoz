using System;
using System.Collections.Generic;
using System.Linq;
using QS.Utilities.Extensions;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Sale;
using VodovozBusiness.Domain.Orders.Cart;
using VodovozBusiness.Domain.Orders.Delivery;

namespace Vodovoz.Core.Application.Orders.Delivery
{
	public class DeliveryRulesHandler : IDeliveryRulesHandler
	{
		private readonly IOnlineCartDistrictRulesGetter _onlineCartDistrictRulesGetter;
		private readonly IDeliveryCostDataFactory _deliveryCostDataFactory;

		public DeliveryRulesHandler(
			IOnlineCartDistrictRulesGetter onlineCartDistrictRulesGetter,
			IDeliveryCostDataFactory deliveryCostDataFactory
			)
		{
			_onlineCartDistrictRulesGetter = onlineCartDistrictRulesGetter ?? throw new ArgumentNullException(nameof(onlineCartDistrictRulesGetter));
			_deliveryCostDataFactory = deliveryCostDataFactory ?? throw new ArgumentNullException(nameof(deliveryCostDataFactory));
		}
		
		public Result<IDeliveryCostData> GetDeliveryCost(IDeliveryRulesRequestContext context)
		{
			var result = _onlineCartDistrictRulesGetter.GetDeliveryRules(context);

			if(result.IsFailure)
			{
				return Result.Failure<IDeliveryCostData>(result.Errors.First());
			}
	
			var districtRules = result.Value;

			return Result.Success(GetDeliveryCost(districtRules));
		}
		
		private IDeliveryCostData GetDeliveryCost(IEnumerable<DistrictRuleItemBase> rules)
		{
			if(rules is null || !rules.Any())
			{
				return _deliveryCostDataFactory.CreateFreeDeliveryCostData();
			}

			var districtRules = rules.ToList();
			districtRules.MergeSort((x, y) =>
			{
				if(x.Price == y.Price)
				{
					return 0;
				}

				//Сортируем по убыванию
				if(x.Price < y.Price)
				{
					return 1;
				}

				return -1;
			});

			return _deliveryCostDataFactory.CreateDeliveryCostData(districtRules, _onlineCartDistrictRulesGetter.CartWaterCounts);
		}
	}
}
