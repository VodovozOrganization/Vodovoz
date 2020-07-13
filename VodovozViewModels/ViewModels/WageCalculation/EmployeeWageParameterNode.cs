using System;
using Vodovoz.Domain.WageCalculation;
using Gamma.Utilities;
namespace Vodovoz.ViewModels.WageCalculation
{
	public class EmployeeWageParameterNode
	{
		public EmployeeWageParameter EmployeeWageParameter { get; private set; }

		public EmployeeWageParameterNode(EmployeeWageParameter wageParameter)
		{
			this.EmployeeWageParameter = wageParameter ?? throw new ArgumentNullException(nameof(wageParameter));
		}

		public int Id => EmployeeWageParameter.Id;
		public string WageType => EmployeeWageParameter.Title;
		public string StartDate => EmployeeWageParameter.StartDate.ToString("G");
		public string EndDate => EmployeeWageParameter.EndDate.HasValue ? EmployeeWageParameter.EndDate.Value.ToString("G") : "";
	}
}
