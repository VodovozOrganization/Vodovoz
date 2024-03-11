using System.Collections.Generic;

namespace Vodovoz.Tools.Logistic
{
	public class DeliveryRuleRow
	{
		public string Volune { get; set; }
		public List<string> DynamicColumns { get; set; }
		public string FreeDeliveryBottlesCount { get; set; }
	}
}
