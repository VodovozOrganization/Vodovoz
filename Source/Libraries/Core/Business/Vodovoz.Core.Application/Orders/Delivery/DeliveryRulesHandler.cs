using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QS.Utilities;
using QS.Utilities.Extensions;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools.Orders;
using VodovozBusiness.Domain.Orders.Cart;
using VodovozBusiness.Domain.Orders.Delivery;

namespace Vodovoz.Core.Application.Orders.Delivery
{
	public class DeliveryRulesHandler : IDeliveryRulesHandler
	{
		private readonly IOnlineCartDistrictRulesGetter _onlineCartDistrictRulesGetter;
		private readonly IDeliveryCostMessageFactory _deliveryCostMessageFactory;

		public DeliveryRulesHandler(
			IOnlineCartDistrictRulesGetter onlineCartDistrictRulesGetter,
			IDeliveryCostMessageFactory deliveryCostMessageFactory
			)
		{
			_onlineCartDistrictRulesGetter = onlineCartDistrictRulesGetter ?? throw new ArgumentNullException(nameof(onlineCartDistrictRulesGetter));
			_deliveryCostMessageFactory = deliveryCostMessageFactory ?? throw new ArgumentNullException(nameof(deliveryCostMessageFactory));
		}
		
		public Result<(decimal? DeliveryPrice, string Message)> GetDeliveryCost(IDeliveryRulesRequestContext context)
		{
			var result = _onlineCartDistrictRulesGetter.GetDeliveryRules(context);

			if(result.IsFailure)
			{
				return Result.Failure<(decimal? DeliveryPrice, string Message)>(result.Errors.First());
			}
	
			var districtRules = result.Value;

			return Result.Success(GetDeliveryCost(districtRules));
		}
		
		private (decimal? DeliveryPrice, string Message) GetDeliveryCost(IEnumerable<DistrictRuleItemBase> rules)
		{
			if(rules is null || !rules.Any())
			{
				return (null, null);
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

			var message = _deliveryCostMessageFactory.CreateDeliveryCostMessage(districtRules, _onlineCartDistrictRulesGetter.CartWaterCounts);
			
			return (districtRules[0].Price, message);
		}
	}

	public class DeliveryCostMessageFactory : IDeliveryCostMessageFactory
	{
		private readonly StringBuilder _sb = new StringBuilder();
		
		public string CreateDeliveryCostMessage(IList<DistrictRuleItemBase> districtRules, IWaterCount waterCounts)
		{
			_sb.Clear();
			var bottlesStingBuilder = new StringBuilder();
			
			var i = 0;

			var total19L = waterCounts.DisposableWater19LCount + waterCounts.NotDisposableWater19LCount;

			do
			{
				bottlesStingBuilder.Clear();
				bottlesStingBuilder.Append($"{districtRules[i].DeliveryPriceRule.Water19LCount - total19L}шт 19л");

				if(waterCounts.DisposableWater6LCount != 0)
				{
					bottlesStingBuilder.Append(" или ");
					bottlesStingBuilder.Append($"{districtRules[i].DeliveryPriceRule.Water6LCount - waterCounts.DisposableWater6LCount}шт 6л");
				}
				
				if(waterCounts.DisposableWater1500mlCount != 0)
				{
					bottlesStingBuilder.Append(" или ");
					bottlesStingBuilder.Append(
						$"{districtRules[i].DeliveryPriceRule.Water1500mlCount - waterCounts.DisposableWater1500mlCount}шт 1.5л");
				}
				
				if(waterCounts.DisposableWater600mlCount != 0)
				{
					bottlesStingBuilder.Append(" или ");
					bottlesStingBuilder.Append($"{districtRules[i].DeliveryPriceRule.Water600mlCount - waterCounts.DisposableWater600mlCount}шт 0.6л");
				}
				
				if(waterCounts.DisposableWater500mlCount != 0)
				{
					bottlesStingBuilder.Append(" или ");
					bottlesStingBuilder.Append($"{districtRules[i].DeliveryPriceRule.Water500mlCount - waterCounts.DisposableWater500mlCount}шт 0.5л");
				}

				bottlesStingBuilder.Append(" бутылок");

				string deliveryMessage = null;
				const string message = "Добавьте в заказ {0}, чтобы доставка стала {1}";

				if(i != districtRules.Count - 1)
				{
					var deliveryPrice = districtRules[i + 1].Price;
					deliveryMessage = deliveryPrice.ToShortCurrencyString();
				}
				else
				{
					deliveryMessage = "бесплатной";
				}
				
				_sb.AppendLine(string.Format(message, bottlesStingBuilder, deliveryMessage));
				i++;

			} while(i < 1); //пока берем только одно правило
			
			return _sb.ToString();
		}
	}
}
