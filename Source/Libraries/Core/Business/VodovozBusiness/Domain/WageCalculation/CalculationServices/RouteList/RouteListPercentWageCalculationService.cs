using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;

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

		public RouteListItemWageCalculationDetails GetWageCalculationDetailsForRouteListItem(IRouteListItemWageCalculationSource src)
		{
			RouteListItemWageCalculationDetails addressWageDetails = new RouteListItemWageCalculationDetails()
			{
				RouteListItemWageCalculationName = wageParameterItem.Title,
				WageCalculationEmployeeCategory = src.EmployeeCategory
			};

			switch(wageParameterItem.PercentWageType)
			{
				case PercentWageTypes.RouteList:
					var itemsSum = src.OrderItemsSource.Sum(i => (i.ActualCount ?? i.InitialCount) * i.Price - i.DiscountMoney);
					var depositsSum = src.OrderDepositItemsSource.Sum(d => (d.ActualCount ?? d.InitialCount) * d.Deposit);
					addressWageDetails.WageCalculationDetailsList.Add(
						new WageCalculationDetailsItem()
						{
							Name = PercentWageTypes.RouteList.GetEnumTitle(),
							Count = 1,
							Price = (itemsSum - depositsSum) * wageParameterItem.RouteListPercent / 100
						});
					break;

				case PercentWageTypes.Service:
					var wageForService = src.OrderItemsSource
						.Where(i => i.IsMasterNomenclature && i.ActualCount.HasValue)
						.Sum(i => i.ActualCount.Value * i.Price * i.PercentForMaster / 100);
					addressWageDetails.WageCalculationDetailsList.Add(
						new WageCalculationDetailsItem()
						{
							Name = PercentWageTypes.Service.GetEnumTitle(),
							Count = 1,
							Price = wageForService
						});
					break;
			}

			return addressWageDetails;
		}
	}
}