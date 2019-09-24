using System;
using Vodovoz.Domain.WageCalculation;
using Gamma.Utilities;
namespace Vodovoz.ViewModels.WageCalculation
{
	public class WageParameterNode
	{
		public WageParameter WageParameter { get; private set; }

		public WageParameterNode(WageParameter wageParameter)
		{
			this.WageParameter = wageParameter ?? throw new ArgumentNullException(nameof(wageParameter));
		}

		public int Id => WageParameter.Id;
		public string WageType => WageParameter.Title;
		public string StartDate => WageParameter.StartDate.ToString("G");
		public string EndDate => WageParameter.EndDate.HasValue ? WageParameter.EndDate.Value.ToString("G") : "";
	}
}
