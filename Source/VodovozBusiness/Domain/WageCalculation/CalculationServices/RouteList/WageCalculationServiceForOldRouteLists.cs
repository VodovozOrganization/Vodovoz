using System;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class WageCalculationServiceForOldRouteLists : IRouteListWageCalculationService
	{
		private readonly IRouteListWageCalculationSource src;

		public WageCalculationServiceForOldRouteLists(IRouteListWageCalculationSource src)
		{
			this.src = src ?? throw new ArgumentNullException(nameof(src));
		}

		public RouteListWageResult CalculateWage()
		{
			//в адресах должны находиться одинаковые методики расчета в рамках одного МЛ
			WageDistrictLevelRate rate = src.ItemSources.Where(x => x.WageCalculationMethodic != null).Select(x => x.WageCalculationMethodic).FirstOrDefault();
			return new RouteListWageResult(src.ItemSources.Sum(x => x.CurrentWage), src.FixedWage, rate);
		}

		public RouteListItemWageResult CalculateWageForRouteListItem(IRouteListItemWageCalculationSource source)
		{
			return new RouteListItemWageResult(source.CurrentWage, source.WageCalculationMethodic);
		}

		public RouteListItemWageCalculationDetails GetWageCalculationDetailsForRouteListItem(IRouteListItemWageCalculationSource source)
		{
			RouteListItemWageCalculationDetails addressWageDetails = new RouteListItemWageCalculationDetails()
			{
				RouteListItemWageCalculationName = "Расчёт ЗП для старых МЛ",
				WageCalculationEmployeeCategory = src.EmployeeCategory
			};

			addressWageDetails.WageCalculationDetailsList.Add(
				new WageCalculationDetailsItem()
				{
					Name = $"Текущая ЗП за адрес",
					Count = 1,
					Price = source.CurrentWage
				});

			return addressWageDetails;
		}
	}
}
