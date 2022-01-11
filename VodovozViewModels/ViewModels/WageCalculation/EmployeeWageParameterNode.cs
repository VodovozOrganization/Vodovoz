using System;
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
				if(EmployeeWageParameter.WageParameterItem is RatesLevelWageParameterItem driverCarParameter &&
				   EmployeeWageParameter.WageParameterItemForOurCars is RatesLevelWageParameterItem companyCarParameter)
				{
					return "Уровень ставок:\n" +
						$"\tДля а/м компании: ({driverCarParameter.WageDistrictLevelRates.Id}) {driverCarParameter.WageDistrictLevelRates.Name}\n" +
						$"\tДля а/м водителя: ({companyCarParameter.WageDistrictLevelRates.Id}) {companyCarParameter.WageDistrictLevelRates.Name}";
				}
				return EmployeeWageParameter.Title;
			}
		}

		public string StartDate => EmployeeWageParameter.StartDate.ToString("G");
		public string EndDate => EmployeeWageParameter.EndDate.HasValue ? EmployeeWageParameter.EndDate.Value.ToString("G") : "";
	}
}
