using System;
using System.Text;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class EmployeeWageParameterNode
	{
		public EmployeeWageParameter EmployeeWageParameter { get; }

		public EmployeeWageParameterNode(EmployeeWageParameter wageParameter)
		{
			EmployeeWageParameter = wageParameter ?? throw new ArgumentNullException(nameof(wageParameter));
		}

		public int Id => EmployeeWageParameter.Id;

		public string Name
		{
			get
			{
				if(EmployeeWageParameter.WageParameterItem is RatesLevelWageParameterItem
				   || EmployeeWageParameter.WageParameterItemForOurCars is RatesLevelWageParameterItem 
				   || EmployeeWageParameter.WageParameterItemForRaskatCars is RatesLevelWageParameterItem)
				{
					var driverCarParameter = EmployeeWageParameter.WageParameterItem as RatesLevelWageParameterItem;
					var companyCarParameter = EmployeeWageParameter.WageParameterItemForOurCars as RatesLevelWageParameterItem;
					var raskatCarParameter = EmployeeWageParameter.WageParameterItemForRaskatCars as RatesLevelWageParameterItem;

					StringBuilder sb = new StringBuilder();
					sb.AppendLine("Уровень ставок:");
					if(driverCarParameter != null)
					{
						sb.AppendLine($"\tДля а/м водителя: ({driverCarParameter.WageDistrictLevelRates?.Id}) {driverCarParameter.WageDistrictLevelRates?.Name}");
					}
					if(companyCarParameter != null)
					{
						sb.AppendLine($"\tДля а/м компании: ({companyCarParameter.WageDistrictLevelRates?.Id}) {companyCarParameter.WageDistrictLevelRates?.Name}");
					}
					if(raskatCarParameter != null)
					{
						sb.AppendLine($"\tДля раскатных авто: ({raskatCarParameter.WageDistrictLevelRates?.Id}) {raskatCarParameter.WageDistrictLevelRates?.Name}");
					}

					return sb.ToString();
				}

				return EmployeeWageParameter.Title;
			}
		}

		public string StartDate => EmployeeWageParameter.StartDate.ToString("G");
		public string EndDate => EmployeeWageParameter.EndDate.HasValue ? EmployeeWageParameter.EndDate.Value.ToString("G") : "";
	}
}
