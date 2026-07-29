using System.Collections.Generic;
using System.Text;
using QS.Utilities;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools.Orders;
using VodovozBusiness.Domain.Orders.Delivery;

namespace Vodovoz.Core.Application.Orders.Delivery
{
	/// <inheritdoc/>
	public class DeliveryCostDataFactory : IDeliveryCostDataFactory
	{
		private readonly StringBuilder _sb = new StringBuilder();
		
		/// <inheritdoc/>
		public IDeliveryCostData CreateDeliveryCostData(IList<DistrictRuleItemBase> districtRules, IWaterCount waterCounts)
		{
			_sb.Clear();
			var bottlesStingBuilder = new StringBuilder();
			
			var i = 0;
			var total19L = waterCounts.DisposableWater19LCount + waterCounts.NotDisposableWater19LCount;
			IMaxVolumeTotalBottles totalBottles = null;

			do
			{
				bottlesStingBuilder.Clear();

				if(total19L != 0)
				{
					var max19LBottles = districtRules[i].DeliveryPriceRule.Water19LCount;
					bottlesStingBuilder.Append($"{max19LBottles - total19L}шт 19л");
					
					TryAddMaxVolumeTotalBottles(max19LBottles, total19L, ref totalBottles);
				}

				if(waterCounts.DisposableWater6LCount != 0)
				{
					var max6LBottles = districtRules[i].DeliveryPriceRule.Water6LCount;
					TryAddOrMessage(bottlesStingBuilder);
					bottlesStingBuilder.Append($"{max6LBottles - waterCounts.DisposableWater6LCount}шт 6л");

					TryAddMaxVolumeTotalBottles(max6LBottles, waterCounts.DisposableWater6LCount, ref totalBottles);
				}
				
				if(waterCounts.DisposableWater1500mlCount != 0)
				{
					var max1500mlBottles = districtRules[i].DeliveryPriceRule.Water1500mlCount;
					TryAddOrMessage(bottlesStingBuilder);
					bottlesStingBuilder.Append($"{max1500mlBottles - waterCounts.DisposableWater1500mlCount}шт 1.5л");
					
					TryAddMaxVolumeTotalBottles(max1500mlBottles, waterCounts.DisposableWater1500mlCount, ref totalBottles);
				}
				
				if(waterCounts.DisposableWater600mlCount != 0)
				{
					var max600mlBottles = districtRules[i].DeliveryPriceRule.Water600mlCount;
					TryAddOrMessage(bottlesStingBuilder);
					bottlesStingBuilder.Append($"{max600mlBottles - waterCounts.DisposableWater600mlCount}шт 0.6л");
					
					TryAddMaxVolumeTotalBottles(max600mlBottles, waterCounts.DisposableWater600mlCount, ref totalBottles);
				}
				
				if(waterCounts.DisposableWater500mlCount != 0)
				{
					var max500mlBottles = districtRules[i].DeliveryPriceRule.Water500mlCount;
					TryAddOrMessage(bottlesStingBuilder);
					bottlesStingBuilder.Append($"{max500mlBottles - waterCounts.DisposableWater500mlCount}шт 0.5л");
					
					TryAddMaxVolumeTotalBottles(max500mlBottles, waterCounts.DisposableWater500mlCount, ref totalBottles);
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
			
			return new DeliveryCostData
			{
				DeliveryPrice = districtRules[0].Price,
				Message = _sb.ToString(),
				MaxVolumeTotalBottles = totalBottles
			};
		}

		public IDeliveryCostData CreateFreeDeliveryCostData()
		{
			return new DeliveryCostData
			{
				DeliveryPrice = null,
				Message = null,
				MaxVolumeTotalBottles = MaxVolumeTotalBottles.Create(1, 1)
			};
		}

		private void TryAddMaxVolumeTotalBottles(decimal maxBottles, decimal currentBottles, ref IMaxVolumeTotalBottles totalBottles)
		{
			totalBottles ??= MaxVolumeTotalBottles.Create(maxBottles, currentBottles);
		}

		private void TryAddOrMessage(StringBuilder bottlesStingBuilder)
		{
			if(bottlesStingBuilder.Length != 0)
			{
				bottlesStingBuilder.Append(" или ");
			}
		}
	}
}
