using System;
using System.Linq;
namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class RouteListPercentWageCalculationService : IRouteListWageCalculationService
	{
		private readonly PercentWageParameter wageParameter;
		private readonly IRouteListWageCalculationSource wageCalculationSource;

		public RouteListPercentWageCalculationService(PercentWageParameter wageParameter, IRouteListWageCalculationSource wageCalculationSource)
		{
			this.wageParameter = wageParameter ?? throw new ArgumentNullException(nameof(wageParameter));
			this.wageCalculationSource = wageCalculationSource ?? throw new ArgumentNullException(nameof(wageCalculationSource));
		}

		public RouteListWageResult CalculateWage()
		{
			var wage = wageCalculationSource.ItemSources.Sum(s => CalculateWageForRouteListItem(s).Wage);
			return new RouteListWageResult(wage, 0);
		}

		public RouteListItemWageResult CalculateWageForRouteListItem(IRouteListItemWageCalculationSource src)
		{
			switch(wageParameter.PercentWageType) {
				case PercentWageTypes.RouteList:
					var itemsSum = src.OrderItemsSource.Sum(i => (i.ActualCount ?? i.InitialCount) * i.Price - i.DiscountMoney);
					var depositsSum = src.OrderDepositItemsSource.Sum(d => (d.ActualCount ?? d.InitialCount) * d.Deposit);
					return new RouteListItemWageResult(
						(itemsSum - depositsSum) * wageParameter.RouteListPercent / 100
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