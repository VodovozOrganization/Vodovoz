using System;
using System.Linq;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class RouteListPercentWageCalculationService : IRouteListWageCalculationService
	{
		private readonly PercentWageParameterItem wageParameterItem;
		private readonly IRouteListWageCalculationSource wageCalculationSource;

		public RouteListPercentWageCalculationService(PercentWageParameterItem wageParameterItem, IRouteListWageCalculationSource wageCalculationSource)
		{
			this.wageParameterItem = wageParameterItem ?? throw new ArgumentNullException(nameof(wageParameterItem));
			this.wageCalculationSource = wageCalculationSource ?? throw new ArgumentNullException(nameof(wageCalculationSource));
		}

		public RouteListWageResult CalculateWage()
		{
			var wage = wageCalculationSource.ItemSources.Sum(s => CalculateWageForRouteListItem(s).Wage);
			return new RouteListWageResult(wage, 0);
		}

		public RouteListItemWageResult CalculateWageForRouteListItem(IRouteListItemWageCalculationSource src)
		{
			switch(wageParameterItem.PercentWageType) {
				case PercentWageTypes.RouteList:
					var itemsSum = src.OrderItemsSource.Sum(i => (i.ActualCount ?? i.InitialCount) * i.Price - i.DiscountMoney);
					var depositsSum = src.OrderDepositItemsSource.Sum(d => (d.ActualCount ?? d.InitialCount) * d.Deposit);
					return new RouteListItemWageResult(
						(itemsSum - depositsSum) * wageParameterItem.RouteListPercent / 100
					);
				case PercentWageTypes.Service:
					var wageForService = src.OrderItemsSource
											.Where(i => i.IsMasterNomenclature && i.ActualCount.HasValue)
											.Sum(i => i.ActualCount.Value * i.Price * i.PercentForMaster / 100);
					return new RouteListItemWageResult(wageForService);
				default:
					throw new NotImplementedException();
			}
		}
	}
}